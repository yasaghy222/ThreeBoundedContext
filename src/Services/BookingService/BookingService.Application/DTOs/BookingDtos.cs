namespace BookingService.Application.DTOs;

public record BookingDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    string UserFullName,
    string Description,
    decimal Amount,
    DateTime BookingDate,
    string Status,
    DateTime CreatedAt
);

public record CreateBookingRequest(
    Guid UserId,
    string Description,
    decimal Amount,
    DateTime BookingDate
);

public record CreateBookingResponse(
    Guid BookingId,
    Guid UserId,
    string Description,
    decimal Amount,
    DateTime BookingDate,
    string Status
);
