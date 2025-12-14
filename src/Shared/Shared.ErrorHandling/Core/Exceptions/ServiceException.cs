using System.Net;
using ErrorHandling.Abstractions;

namespace ErrorHandling.Core.Exceptions;

public abstract class ServiceException : Exception, IServiceException
{
    protected ServiceException(
        string message,
        HttpStatusCode statusCode,
        string errorCode,
        IReadOnlyDictionary<string, string[]>? errors = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Errors = errors;
    }

    public HttpStatusCode StatusCode { get; }

    public string ErrorCode { get; }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }
}
