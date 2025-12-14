using System.Net;

namespace ErrorHandling.Core.Exceptions;

public class UnauthorizedException : ServiceException
{
    public UnauthorizedException(
        string message,
        string errorCode = "unauthorized",
        Exception? innerException = null)
        : base(message, HttpStatusCode.Unauthorized, errorCode, null, innerException)
    {
    }
}
