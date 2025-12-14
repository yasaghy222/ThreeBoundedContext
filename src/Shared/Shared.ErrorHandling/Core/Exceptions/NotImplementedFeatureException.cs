using System.Net;

namespace ErrorHandling.Core.Exceptions;

public sealed class NotImplementedFeatureException : ServiceException
{
    public NotImplementedFeatureException(
        string message = "This feature is not implemented yet.",
        string errorCode = "feature_not_implemented",
        IReadOnlyDictionary<string, string[]>? errors = null,
        Exception? innerException = null)
        : base(message, HttpStatusCode.NotImplemented, errorCode, errors, innerException)
    {
    }
}

