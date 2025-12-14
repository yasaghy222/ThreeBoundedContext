using System.Net;

namespace ErrorHandling.Abstractions;

public interface IServiceException
{
    HttpStatusCode StatusCode { get; }
    string ErrorCode { get; }
    string Message { get; }
    IReadOnlyDictionary<string, string[]>? Errors { get; }
}
