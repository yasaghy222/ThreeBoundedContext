using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Logging.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Logging.Core.Correlation;

internal class CorrelationIdMiddleware : IMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private const string ErrorLoggedItemKey = "__logging_error_logged";
    private readonly ICorrelationIdAccessor _accessor;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(ICorrelationIdAccessor accessor, ILogger<CorrelationIdMiddleware> logger)
    {
        _accessor = accessor;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = ResolveCorrelationId(context);
        _accessor.CorrelationId = correlationId;

        context.Response.Headers[HeaderName] = correlationId;
        context.TraceIdentifier = correlationId;
        Activity.Current?.SetTag("correlation.id", correlationId);

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            var timer = Stopwatch.StartNew();
            try
            {
                await next(context);
            }
            finally
            {
                timer.Stop();
                var skipLogging = context.Items.TryGetValue(ErrorLoggedItemKey, out var flagged) && flagged is true;
                if (!skipLogging)
                {
                    var status = context.Response.StatusCode;
                    var method = context.Request.Method;
                    var path = context.Request.Path.Value ?? "/";
                    var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
                    var elapsedMs = timer.Elapsed.TotalMilliseconds;

                    if (status < StatusCodes.Status400BadRequest)
                    {
                        _logger.LogInformation(
                            "Request completed: {Method} {Path}{Query} => {StatusCode} in {Duration} ms",
                            method,
                            path,
                            query,
                            status,
                            elapsedMs);
                    }
                    else if (status < StatusCodes.Status500InternalServerError)
                    {
                        _logger.LogWarning(
                            "Request returned client error: {Method} {Path}{Query} => {StatusCode} in {Duration} ms",
                            method,
                            path,
                            query,
                            status,
                            elapsedMs);
                    }
                    else
                    {
                        _logger.LogError(
                            "Request returned server error: {Method} {Path}{Query} => {StatusCode} in {Duration} ms",
                            method,
                            path,
                            query,
                            status,
                            elapsedMs);
                    }
                }
            }
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var existing) && !string.IsNullOrWhiteSpace(existing))
        {
            return existing.ToString();
        }

        return Guid.NewGuid().ToString("N");
    }
}
