using FinanceService.Domain.Common;
using FinanceService.Domain.ValueObjects;

namespace FinanceService.Domain.Entities;

public class Invoice : Entity
{
	public Guid BookingId { get; private set; }
	public Guid UserId { get; private set; }
	public string UserEmail { get; private set; } = null!;
	public string UserFullName { get; private set; } = null!;
	public string BookingDescription { get; private set; } = null!;
	public decimal Amount { get; private set; }
	public DateTime BookingDate { get; private set; }
	public string InvoiceNumber { get; private set; } = null!;
	public InvoiceStatus Status { get; private set; }
	public DateTime CreatedAt { get; private set; }
	public DateTime? PaidAt { get; private set; }
	public DateTime DueDate { get; private set; }

	private Invoice() { } // EF Core

	private Invoice(
	    Guid id,
	    Guid bookingId,
	    Guid userId,
	    string userEmail,
	    string userFullName,
	    string bookingDescription,
	    decimal amount,
	    DateTime bookingDate) : base(id)
	{
		BookingId = bookingId;
		UserId = userId;
		UserEmail = userEmail;
		UserFullName = userFullName;
		BookingDescription = bookingDescription;
		Amount = amount;
		BookingDate = bookingDate;
		InvoiceNumber = GenerateInvoiceNumber();
		Status = InvoiceStatus.Pending;
		CreatedAt = DateTime.UtcNow;
		DueDate = DateTime.UtcNow.AddDays(30);
	}

	public static Invoice Create(
	    Guid bookingId,
	    Guid userId,
	    string userEmail,
	    string userFullName,
	    string bookingDescription,
	    decimal amount,
	    DateTime bookingDate)
	{
		return new Invoice(
		    Guid.NewGuid(),
		    bookingId,
		    userId,
		    userEmail,
		    userFullName,
		    bookingDescription,
		    amount,
		    bookingDate);
	}

	public void MarkAsPaid()
	{
		if (Status == InvoiceStatus.Paid)
			throw new InvalidOperationException("Invoice is already paid");

		if (Status == InvoiceStatus.Cancelled)
			throw new InvalidOperationException("Cannot pay a cancelled invoice");

		Status = InvoiceStatus.Paid;
		PaidAt = DateTime.UtcNow;
	}

	public void Cancel()
	{
		if (Status == InvoiceStatus.Paid)
			throw new InvalidOperationException("Cannot cancel a paid invoice");

		Status = InvoiceStatus.Cancelled;
	}

	public void MarkAsOverdue()
	{
		if (Status == InvoiceStatus.Pending && DateTime.UtcNow > DueDate)
		{
			Status = InvoiceStatus.Overdue;
		}
	}

	private static string GenerateInvoiceNumber()
	{
		return $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
	}
}
