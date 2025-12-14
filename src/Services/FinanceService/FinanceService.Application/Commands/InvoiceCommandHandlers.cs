using MediatR;
using Microsoft.Extensions.Logging;
using FinanceService.Application.DTOs;
using FinanceService.Domain.Entities;
using FinanceService.Domain.Repositories;

namespace FinanceService.Application.Commands;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, CreateInvoiceResponse>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger<CreateInvoiceCommandHandler> _logger;

    public CreateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        ILogger<CreateInvoiceCommandHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _logger = logger;
    }

    public async Task<CreateInvoiceResponse> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        // Idempotency check: If invoice already exists for this booking, return existing
        var existingInvoice = await _invoiceRepository.GetByBookingIdAsync(request.BookingId, cancellationToken);
        
        if (existingInvoice != null)
        {
            _logger.LogInformation(
                "Invoice already exists for booking {BookingId}. Returning existing invoice {InvoiceId}",
                request.BookingId,
                existingInvoice.Id);

            return new CreateInvoiceResponse(
                existingInvoice.Id,
                existingInvoice.InvoiceNumber,
                existingInvoice.Amount,
                existingInvoice.DueDate
            );
        }

        // Create new invoice
        var invoice = Invoice.Create(
            request.BookingId,
            request.UserId,
            request.UserEmail,
            request.UserFullName,
            request.BookingDescription,
            request.Amount,
            request.BookingDate
        );

        await _invoiceRepository.AddAsync(invoice, cancellationToken);

        _logger.LogInformation(
            "Created invoice {InvoiceId} for booking {BookingId}",
            invoice.Id,
            request.BookingId);

        return new CreateInvoiceResponse(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.Amount,
            invoice.DueDate
        );
    }
}

public class MarkInvoicePaidCommandHandler : IRequestHandler<MarkInvoicePaidCommand, bool>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger<MarkInvoicePaidCommandHandler> _logger;

    public MarkInvoicePaidCommandHandler(
        IInvoiceRepository invoiceRepository,
        ILogger<MarkInvoicePaidCommandHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(MarkInvoicePaidCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
        
        if (invoice == null)
        {
            _logger.LogWarning("Invoice {InvoiceId} not found", request.InvoiceId);
            return false;
        }

        invoice.MarkAsPaid();
        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

        _logger.LogInformation("Invoice {InvoiceId} marked as paid", request.InvoiceId);
        return true;
    }
}
