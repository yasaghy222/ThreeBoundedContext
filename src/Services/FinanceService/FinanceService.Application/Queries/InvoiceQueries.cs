using MediatR;
using FinanceService.Application.DTOs;

namespace FinanceService.Application.Queries;

public record GetInvoiceByIdQuery(Guid InvoiceId) : IRequest<InvoiceDto?>;

public record GetInvoiceByBookingIdQuery(Guid BookingId) : IRequest<InvoiceDto?>;

public record GetInvoicesByUserIdQuery(Guid UserId) : IRequest<IEnumerable<InvoiceDto>>;

public record GetAllInvoicesQuery : IRequest<IEnumerable<InvoiceDto>>;
