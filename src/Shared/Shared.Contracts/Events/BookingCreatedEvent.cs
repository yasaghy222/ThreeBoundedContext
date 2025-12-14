namespace Shared.Contracts.Events;

public record BookingCreatedEvent(
    Guid BookingId,
    Guid UserId,
    string UserEmail,
    string UserFullName,
    string Description,
    decimal Amount,
    DateTime BookingDate,
    DateTime CreatedAt
);
