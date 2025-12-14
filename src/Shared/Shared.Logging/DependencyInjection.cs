using Logging.Abstractions;
using Logging.Core.Correlation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logging;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddLoggingExtension(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options =>
        {
            options.IncludeScopes = true;
        });
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();
        builder.Services.AddSingleton<CorrelationIdMiddleware>();

        return builder;
    }

    public static IApplicationBuilder UseLoggingExtension(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
