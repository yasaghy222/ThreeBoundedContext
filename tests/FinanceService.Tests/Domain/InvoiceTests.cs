using ErrorHandling.Core.Exceptions;
using FluentAssertions;
using FinanceService.Domain.Entities;
using FinanceService.Domain.ValueObjects;

namespace FinanceService.Tests.Domain;

public class InvoiceTests
{
	[Fact]
	public void Create_ValidInput_ShouldCreateInvoice()
	{
		// Arrange
		var bookingId = Guid.NewGuid();
		var userId = Guid.NewGuid();
		var amount = 150.00m;

		// Act
		var invoice = Invoice.Create(
		    bookingId,
		    userId,
		    "test@example.com",
		    "Test User",
		    "Test Booking",
		    amount,
		    DateTime.UtcNow);

		// Assert
		invoice.Should().NotBeNull();
		invoice.Id.Should().NotBeEmpty();
		invoice.BookingId.Should().Be(bookingId);
		invoice.UserId.Should().Be(userId);
		invoice.Amount.Should().Be(amount);
		invoice.Status.Should().Be(InvoiceStatus.Pending);
		invoice.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
		invoice.PaidAt.Should().BeNull();
		invoice.InvoiceNumber.Should().StartWith("INV-");
	}

	[Fact]
	public void MarkAsPaid_ShouldChangeStatusToPaid()
	{
		// Arrange
		var invoice = Invoice.Create(
		    Guid.NewGuid(),
		    Guid.NewGuid(),
		    "test@example.com",
		    "Test User",
		    "Test Booking",
		    100m,
		    DateTime.UtcNow);

		// Act
		invoice.MarkAsPaid();

		// Assert
		invoice.Status.Should().Be(InvoiceStatus.Paid);
		invoice.PaidAt.Should().NotBeNull();
		invoice.PaidAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
	}

	[Fact]
	public void Cancel_ShouldChangeStatusToCancelled()
	{
		// Arrange
		var invoice = Invoice.Create(
		    Guid.NewGuid(),
		    Guid.NewGuid(),
		    "test@example.com",
		    "Test User",
		    "Test Booking",
		    100m,
		    DateTime.UtcNow);

		// Act
		invoice.Cancel();

		// Assert
		invoice.Status.Should().Be(InvoiceStatus.Cancelled);
	}

	[Fact]
	public void MarkAsPaid_AlreadyPaid_ShouldThrowException()
	{
		// Arrange
		var invoice = Invoice.Create(
		    Guid.NewGuid(),
		    Guid.NewGuid(),
		    "test@example.com",
		    "Test User",
		    "Test Booking",
		    100m,
		    DateTime.UtcNow);
		invoice.MarkAsPaid();

		// Act & Assert
		Assert.Throws<BadRequestException>(() => invoice.MarkAsPaid());
	}

	[Fact]
	public void MarkAsPaid_CancelledInvoice_ShouldThrowException()
	{
		// Arrange
		var invoice = Invoice.Create(
		    Guid.NewGuid(),
		    Guid.NewGuid(),
		    "test@example.com",
		    "Test User",
		    "Test Booking",
		    100m,
		    DateTime.UtcNow);
		invoice.Cancel();

		// Act & Assert
		Assert.Throws<BadRequestException>(() => invoice.MarkAsPaid());
	}

	[Fact]
	public void Create_ShouldGenerateUniqueInvoiceNumber()
	{
		// Arrange & Act
		var invoice1 = Invoice.Create(
		    Guid.NewGuid(),
		    Guid.NewGuid(),
		    "test@example.com",
		    "Test User",
		    "Test Booking",
		    100m,
		    DateTime.UtcNow);

		var invoice2 = Invoice.Create(
		    Guid.NewGuid(),
		    Guid.NewGuid(),
		    "test@example.com",
		    "Test User",
		    "Test Booking",
		    200m,
		    DateTime.UtcNow);

		// Assert
		invoice1.InvoiceNumber.Should().NotBe(invoice2.InvoiceNumber);
	}

	[Fact]
	public void Create_ShouldSetDueDate30DaysFromNow()
	{
		// Arrange & Act
		var invoice = Invoice.Create(
		    Guid.NewGuid(),
		    Guid.NewGuid(),
		    "test@example.com",
		    "Test User",
		    "Test Booking",
		    100m,
		    DateTime.UtcNow);

		// Assert
		invoice.DueDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
	}
}
