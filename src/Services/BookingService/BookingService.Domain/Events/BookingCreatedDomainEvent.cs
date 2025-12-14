using BookingService.Domain.Common;

namespace BookingService.Domain.Events;

public record BookingCreatedDomainEvent(
    Guid BookingId,
    Guid UserId,
    string UserEmail,
    string UserFullName,
    string Description,
    decimal Amount,
    DateTime BookingDate,
    DateTime CreatedAt
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
