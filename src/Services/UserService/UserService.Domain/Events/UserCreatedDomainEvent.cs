using UserService.Domain.Common;

namespace UserService.Domain.Events;

public record UserCreatedDomainEvent(
    Guid UserId,
    string Email,
    string FullName,
    DateTime CreatedAt
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
