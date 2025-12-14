using System.Net;
using System.Text.Json;
using System.Linq;
using ErrorHandling.Abstractions;
using ErrorHandling.Core.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ErrorHandling.Core;

public class ExceptionHandlingMiddleware
{
    private const string ErrorLoggedItemKey = "__logging_error_logged";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ServiceException ex)
        {
            await HandleKnownExceptionAsync(context, ex);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (HttpRequestException ex)
        {
            var serviceException = new ExternalServiceException(
                "Upstream service is unavailable.",
                ex.StatusCode ?? HttpStatusCode.BadGateway,
                innerException: ex);
            await HandleKnownExceptionAsync(context, serviceException);
        }
        catch (Exception ex)
        {
            context.Items[ErrorLoggedItemKey] = true;
            _logger.LogError(ex, "Unhandled exception encountered for {Path}.", context.Request.Path);
            var problem = CreateProblemDetails(
                context,
                status: StatusCodes.Status500InternalServerError,
                title: "Unexpected server error.",
                detail: "An unexpected error occurred. Please try again later.",
                errorCode: "server_error");
            await WriteProblemDetailsAsync(context, problem);
        }
    }

    private async Task HandleKnownExceptionAsync(HttpContext context, IServiceException ex)
    {
        var status = (int)ex.StatusCode;
        if (status >= StatusCodes.Status500InternalServerError)
        {
            context.Items[ErrorLoggedItemKey] = true;
            _logger.LogError(ex as Exception, "{ErrorCode} encountered for {Path}.", ex.ErrorCode, context.Request.Path);
        }
        else
        {
            context.Items[ErrorLoggedItemKey] = true;
            _logger.LogWarning(ex as Exception, "{ErrorCode} encountered for {Path}.", ex.ErrorCode, context.Request.Path);
        }

        ProblemDetails problem = ex.Errors is { Count: > 0 } errors
            ? CreateValidationProblemDetails(
                context,
                status,
                ex.Message,
                BuildValidationDetail(errors),
                ex.ErrorCode,
                errors)
            : CreateProblemDetails(context, status, ex.Message, ex.Message, ex.ErrorCode);

        await WriteProblemDetailsAsync(context, problem);
    }

    private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        context.Items[ErrorLoggedItemKey] = true;
        _logger.LogWarning("validation_error encountered for {Path}.", context.Request.Path);

        var detail = BuildValidationDetail(errors);
        var problem = CreateValidationProblemDetails(
            context,
            StatusCodes.Status400BadRequest,
            "Validation failed.",
            detail,
            "validation_error",
            errors);

        await WriteProblemDetailsAsync(context, problem);
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext context,
        int status,
        string title,
        string detail,
        string errorCode)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.io/{status}",
            Instance = context.Request.Path
        };

        problem.Extensions["errorCode"] = errorCode;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        return problem;
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        int status,
        string title,
        string detail,
        string errorCode,
        IReadOnlyDictionary<string, string[]> errors)
    {
        var problem = new ValidationProblemDetails(errors.ToDictionary(k => k.Key, v => v.Value))
        {
            Status = status,
            Title = title,
            Detail = string.IsNullOrWhiteSpace(detail)
                ? "Refer to the errors collection for additional details."
                : detail,
            Type = $"https://httpstatuses.io/{status}",
            Instance = context.Request.Path
        };

        problem.Extensions["errorCode"] = errorCode;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        return problem;
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, ProblemDetails problem)
    {
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }

    private static string BuildValidationDetail(IReadOnlyDictionary<string, string[]> errors)
    {
        var entries = errors
            .Select(pair =>
            {
                var field = string.IsNullOrWhiteSpace(pair.Key) ? "General" : pair.Key;
                var msgs = (pair.Value ?? Array.Empty<string>())
                    .Where(msg => !string.IsNullOrWhiteSpace(msg))
                    .Distinct()
                    .ToArray();
                return msgs.Length == 0 ? null : $"{field}: {string.Join(", ", msgs)}";
            })
            .Where(entry => entry is not null)
            .ToArray();

        return entries.Length == 0 ? string.Empty : string.Join(" | ", entries!);
    }
}
