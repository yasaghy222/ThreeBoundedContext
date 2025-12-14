namespace Logging.Abstractions;

public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; set; }
}
