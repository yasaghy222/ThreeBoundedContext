using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Shared.Swagger.Filters;
using Shared.Swagger.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Shared.Swagger;

public static class DependencyInjection
{
    public static IServiceCollection AddSwaggerExtension(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var section = configuration.GetSection("Swagger");
        services.Configure<SwaggerOptions>(section);
        var swaggerOptions = section.Get<SwaggerOptions>() ?? new SwaggerOptions();

        if (!swaggerOptions.Enabled)
        {
            return services;
        }

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(setup =>
        {
            foreach (var document in swaggerOptions.GetDocumentsOrDefault(environment.ApplicationName))
            {
                var info = new OpenApiInfo
                {
                    Title = document.Title ?? $"{environment.ApplicationName} API",
                    Version = string.IsNullOrWhiteSpace(document.Version) ? "v1" : document.Version,
                    Description = document.Description,
                };

                if (!string.IsNullOrWhiteSpace(document.ContactName) || !string.IsNullOrWhiteSpace(document.ContactEmail))
                {
                    info.Contact = new OpenApiContact
                    {
                        Name = document.ContactName,
                        Email = document.ContactEmail
                    };
                }

                if (!string.IsNullOrWhiteSpace(document.LicenseName))
                {
                    info.License = new OpenApiLicense
                    {
                        Name = document.LicenseName,
                        Url = string.IsNullOrWhiteSpace(document.LicenseUrl) ? null : new Uri(document.LicenseUrl)
                    };
                }

                setup.SwaggerDoc(document.Name, info);
            }

            if (swaggerOptions.Security?.EnableBearer ?? false)
            {
                var schemeName = swaggerOptions.Security.Scheme;
                setup.AddSecurityDefinition(schemeName, new OpenApiSecurityScheme
                {
                    Description = swaggerOptions.Security.Description,
                    Name = swaggerOptions.Security.HeaderName,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = swaggerOptions.Security.Scheme.ToLowerInvariant(),
                    BearerFormat = swaggerOptions.Security.BearerFormat
                });

                setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = schemeName
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            }

            setup.OperationFilter<DefaultValuesOperationFilter>();
            setup.OperationFilter<ResponseTypesOperationFilter>();
            setup.OperationFilter<PersianSummaryOperationFilter>();
            setup.SchemaFilter<EnumSchemaFilter>();
            setup.CustomSchemaIds(type =>
            {
                var targetType = type;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    targetType = Nullable.GetUnderlyingType(type)!;
                }

                if (targetType.IsEnum)
                {
                    return targetType.Name;
                }

                return type.FullName?.Replace("+", ".");
            });
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerExtension(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<SwaggerOptions>>().Value;
        if (!options.Enabled)
        {
            return app;
        }

        var routeTemplate = string.IsNullOrWhiteSpace(options.RouteTemplate)
            ? "swagger/{documentName}/swagger.json"
            : options.RouteTemplate;
        var normalizedRouteTemplate = routeTemplate.TrimStart('/');

        app.UseSwagger(setup =>
        {
            setup.RouteTemplate = normalizedRouteTemplate;
        });

        var ui = options.Ui ?? new SwaggerUiOptions();
        if (ui.Enable && (environment.IsDevelopment() || ui.ExposeInProduction))
        {
            app.UseSwaggerUI(setup =>
            {
                setup.RoutePrefix = ui.RoutePrefix ?? string.Empty;
                if (ui.DisplayRequestDuration)
                {
                    setup.DisplayRequestDuration();
                }

                if (ui.ShowExtensions)
                {
                    setup.ShowExtensions();
                }

                setup.DefaultModelsExpandDepth(ui.DefaultModelsExpandDepth);
                setup.DocExpansion(DocExpansion.None);

                if (ui.EnableFilter)
                {
                    setup.EnableFilter();
                }

                // Use Swagger UI defaults (no custom operation sorting).

                foreach (var document in options.GetDocumentsOrDefault(environment.ApplicationName))
                {
                    var endpoint = "/" + normalizedRouteTemplate.Replace("{documentName}", document.Name);
                    setup.SwaggerEndpoint(endpoint, document.Title ?? document.Name);
                }
            });
        }

        return app;
    }

    // Removed all custom server-side and UI ordering to keep Swagger default ordering.

}
