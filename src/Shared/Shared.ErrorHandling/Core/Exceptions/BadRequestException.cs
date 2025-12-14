using System.Net;

namespace ErrorHandling.Core.Exceptions;

public class BadRequestException : ServiceException
{
    public BadRequestException(
        string message,
        string errorCode = "bad_request",
        IReadOnlyDictionary<string, string[]>? errors = null,
        Exception? innerException = null)
        : base(message, HttpStatusCode.BadRequest, errorCode, errors, innerException)
    {
    }
}
