using MediatR;
using FinanceService.Application.DTOs;

namespace FinanceService.Application.Commands;

public record CreateInvoiceCommand(
    Guid BookingId,
    Guid UserId,
    string UserEmail,
    string UserFullName,
    string BookingDescription,
    decimal Amount,
    DateTime BookingDate
) : IRequest<CreateInvoiceResponse>;

public record MarkInvoicePaidCommand(Guid InvoiceId) : IRequest<bool>;
