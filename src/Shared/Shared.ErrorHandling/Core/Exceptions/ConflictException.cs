using System.Net;

namespace ErrorHandling.Core.Exceptions;

public class ConflictException : ServiceException
{
    public ConflictException(
        string message,
        string errorCode = "conflict",
        Exception? innerException = null)
        : base(message, HttpStatusCode.Conflict, errorCode, null, innerException)
    {
    }
}
