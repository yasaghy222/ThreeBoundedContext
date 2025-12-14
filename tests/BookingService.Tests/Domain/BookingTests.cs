using FluentAssertions;
using BookingService.Domain.Entities;
using BookingService.Domain.Events;
using BookingService.Domain.ValueObjects;

namespace BookingService.Tests.Domain;

public class BookingTests
{
	[Fact]
	public void Create_ValidInput_ShouldCreateBooking()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var userEmail = "test@example.com";
		var userFullName = "Test User";
		var description = "Test Service";
		var amount = 150.00m;
		var bookingDate = DateTime.UtcNow.AddDays(1);

		// Act
		var booking = Booking.Create(userId, userEmail, userFullName, description, amount, bookingDate);

		// Assert
		booking.Should().NotBeNull();
		booking.Id.Should().NotBeEmpty();
		booking.UserId.Should().Be(userId);
		booking.UserEmail.Should().Be(userEmail);
		booking.UserFullName.Should().Be(userFullName);
		booking.Description.Should().Be(description);
		booking.Amount.Should().Be(amount);
		booking.BookingDate.Should().Be(bookingDate);
		booking.Status.Should().Be(BookingStatus.Pending);
		booking.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
	}

	[Fact]
	public void Create_ShouldRaiseDomainEvent()
	{
		// Arrange & Act
		var userId = Guid.NewGuid();
		var booking = Booking.Create(userId, "test@example.com", "Test User", "Test Service", 100m, DateTime.UtcNow.AddDays(1));

		// Assert
		booking.DomainEvents.Should().HaveCount(1);
		booking.DomainEvents.First().Should().BeOfType<BookingCreatedDomainEvent>();

		var domainEvent = (BookingCreatedDomainEvent)booking.DomainEvents.First();
		domainEvent.BookingId.Should().Be(booking.Id);
		domainEvent.UserId.Should().Be(userId);
		domainEvent.Amount.Should().Be(100m);
	}

	[Fact]
	public void Confirm_ShouldChangeStatusToConfirmed()
	{
		// Arrange
		var booking = Booking.Create(Guid.NewGuid(), "test@example.com", "Test User", "Test", 100m, DateTime.UtcNow.AddDays(1));
		booking.ClearDomainEvents();

		// Act
		booking.Confirm();

		// Assert
		booking.Status.Should().Be(BookingStatus.Confirmed);
		booking.UpdatedAt.Should().NotBeNull();
	}

	[Fact]
	public void Cancel_ShouldChangeStatusToCancelled()
	{
		// Arrange
		var booking = Booking.Create(Guid.NewGuid(), "test@example.com", "Test User", "Test", 100m, DateTime.UtcNow.AddDays(1));
		booking.ClearDomainEvents();

		// Act
		booking.Cancel();

		// Assert
		booking.Status.Should().Be(BookingStatus.Cancelled);
		booking.UpdatedAt.Should().NotBeNull();
	}
}
