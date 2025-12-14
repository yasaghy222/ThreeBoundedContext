using System.Linq;
using ErrorHandling.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ErrorHandling;

public static class DependencyInjection
{
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value!.Errors.Select(error => error.ErrorMessage).Distinct().ToArray());

                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Validation");
                logger.LogWarning("validation_error encountered for {Path}.", context.HttpContext.Request.Path);

                var problem = new ValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed.",
                    Detail = "Refer to the errors collection for additional details.",
                    Type = "https://httpstatuses.io/400",
                    Instance = context.HttpContext.Request.Path
                };

                problem.Extensions["errorCode"] = "validation_error";
                problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                return new BadRequestObjectResult(problem)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        });

        return services;
    }

    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
