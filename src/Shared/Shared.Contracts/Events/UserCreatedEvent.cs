namespace Shared.Contracts.Events;

public record UserCreatedEvent(
    Guid UserId,
    string Email,
    string FullName,
    DateTime CreatedAt
);
