using System.Net;

namespace ErrorHandling.Core.Exceptions;

public class NotFoundException : ServiceException
{
    public NotFoundException(
        string message,
        string errorCode = "not_found",
        Exception? innerException = null)
        : base(message, HttpStatusCode.NotFound, errorCode, null, innerException)
    {
    }
}
