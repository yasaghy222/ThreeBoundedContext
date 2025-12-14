using Logging.Abstractions;
using Logging.Core.Correlation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Logging;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddLoggingExtension(this WebApplicationBuilder builder, string serviceName)
    {
        var seqUrl = builder.Configuration["SEQ_URL"] ?? builder.Configuration["Serilog:WriteTo:1:Args:serverUrl"] ?? "http://localhost:5341";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("ServiceName", serviceName)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Seq(seqUrl)
            .CreateLogger();

        builder.Host.UseSerilog();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();
        builder.Services.AddSingleton<CorrelationIdMiddleware>();

        return builder;
    }

    public static IApplicationBuilder UseLoggingExtension(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var correlationIdAccessor = httpContext.RequestServices.GetService<ICorrelationIdAccessor>();
                if (correlationIdAccessor?.CorrelationId is not null)
                {
                    diagnosticContext.Set("CorrelationId", correlationIdAccessor.CorrelationId);
                }
            };
        });

        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
