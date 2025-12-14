namespace FinanceService.Application.DTOs;

public record InvoiceDto(
    Guid Id,
    Guid BookingId,
    Guid UserId,
    string UserEmail,
    string UserFullName,
    string BookingDescription,
    decimal Amount,
    DateTime BookingDate,
    string InvoiceNumber,
    string Status,
    DateTime CreatedAt,
    DateTime? PaidAt,
    DateTime DueDate
);

public record CreateInvoiceRequest(
    Guid BookingId,
    Guid UserId,
    string UserEmail,
    string UserFullName,
    string BookingDescription,
    decimal Amount,
    DateTime BookingDate
);

public record CreateInvoiceResponse(
    Guid InvoiceId,
    string InvoiceNumber,
    decimal Amount,
    DateTime DueDate
);
