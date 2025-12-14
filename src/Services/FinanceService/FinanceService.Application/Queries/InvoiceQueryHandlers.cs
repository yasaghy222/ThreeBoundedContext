using MediatR;
using FinanceService.Application.DTOs;
using FinanceService.Domain.Entities;
using FinanceService.Domain.Repositories;

namespace FinanceService.Application.Queries;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceByIdQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
        return invoice == null ? null : MapToDto(invoice);
    }

    private static InvoiceDto MapToDto(Invoice invoice) => new(
        invoice.Id,
        invoice.BookingId,
        invoice.UserId,
        invoice.UserEmail,
        invoice.UserFullName,
        invoice.BookingDescription,
        invoice.Amount,
        invoice.BookingDate,
        invoice.InvoiceNumber,
        invoice.Status.ToString(),
        invoice.CreatedAt,
        invoice.PaidAt,
        invoice.DueDate
    );
}

public class GetInvoiceByBookingIdQueryHandler : IRequestHandler<GetInvoiceByBookingIdQuery, InvoiceDto?>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceByBookingIdQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceDto?> Handle(GetInvoiceByBookingIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByBookingIdAsync(request.BookingId, cancellationToken);
        return invoice == null ? null : new InvoiceDto(
            invoice.Id,
            invoice.BookingId,
            invoice.UserId,
            invoice.UserEmail,
            invoice.UserFullName,
            invoice.BookingDescription,
            invoice.Amount,
            invoice.BookingDate,
            invoice.InvoiceNumber,
            invoice.Status.ToString(),
            invoice.CreatedAt,
            invoice.PaidAt,
            invoice.DueDate
        );
    }
}

public class GetInvoicesByUserIdQueryHandler : IRequestHandler<GetInvoicesByUserIdQuery, IEnumerable<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoicesByUserIdQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<InvoiceDto>> Handle(GetInvoicesByUserIdQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        
        return invoices.Select(i => new InvoiceDto(
            i.Id,
            i.BookingId,
            i.UserId,
            i.UserEmail,
            i.UserFullName,
            i.BookingDescription,
            i.Amount,
            i.BookingDate,
            i.InvoiceNumber,
            i.Status.ToString(),
            i.CreatedAt,
            i.PaidAt,
            i.DueDate
        ));
    }
}

public class GetAllInvoicesQueryHandler : IRequestHandler<GetAllInvoicesQuery, IEnumerable<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetAllInvoicesQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<InvoiceDto>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _invoiceRepository.GetAllAsync(cancellationToken);
        
        return invoices.Select(i => new InvoiceDto(
            i.Id,
            i.BookingId,
            i.UserId,
            i.UserEmail,
            i.UserFullName,
            i.BookingDescription,
            i.Amount,
            i.BookingDate,
            i.InvoiceNumber,
            i.Status.ToString(),
            i.CreatedAt,
            i.PaidAt,
            i.DueDate
        ));
    }
}
