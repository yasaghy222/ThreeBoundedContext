using System.Net;

namespace ErrorHandling.Core.Exceptions;

public class ExternalServiceException : ServiceException
{
    public ExternalServiceException(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadGateway,
        string errorCode = "external_service_error",
        Exception? innerException = null)
        : base(message, statusCode, errorCode, null, innerException)
    {
    }
}
