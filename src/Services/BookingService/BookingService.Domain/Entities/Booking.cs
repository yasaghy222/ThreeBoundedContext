using BookingService.Domain.Common;
using BookingService.Domain.Events;
using BookingService.Domain.ValueObjects;

namespace BookingService.Domain.Entities;

public class Booking : Entity
{
	public Guid UserId { get; private set; }
	public string UserEmail { get; private set; } = null!;
	public string UserFullName { get; private set; } = null!;
	public string Description { get; private set; } = null!;
	public decimal Amount { get; private set; }
	public DateTime BookingDate { get; private set; }
	public BookingStatus Status { get; private set; }
	public DateTime CreatedAt { get; private set; }
	public DateTime? UpdatedAt { get; private set; }

	private Booking() { } // EF Core

	private Booking(
	    Guid id,
	    Guid userId,
	    string userEmail,
	    string userFullName,
	    string description,
	    decimal amount,
	    DateTime bookingDate) : base(id)
	{
		UserId = userId;
		UserEmail = userEmail;
		UserFullName = userFullName;
		Description = description;
		Amount = amount;
		BookingDate = bookingDate;
		Status = BookingStatus.Pending;
		CreatedAt = DateTime.UtcNow;
	}

	public static Booking Create(
	    Guid userId,
	    string userEmail,
	    string userFullName,
	    string description,
	    decimal amount,
	    DateTime bookingDate)
	{
		var booking = new Booking(
		    Guid.NewGuid(),
		    userId,
		    userEmail,
		    userFullName,
		    description,
		    amount,
		    bookingDate);

		booking.AddDomainEvent(new BookingCreatedDomainEvent(
		    booking.Id,
		    booking.UserId,
		    booking.UserEmail,
		    booking.UserFullName,
		    booking.Description,
		    booking.Amount,
		    booking.BookingDate,
		    booking.CreatedAt
		));

		return booking;
	}

	public void Confirm()
	{
		if (Status != BookingStatus.Pending)
			throw new InvalidOperationException("Only pending bookings can be confirmed");

		Status = BookingStatus.Confirmed;
		UpdatedAt = DateTime.UtcNow;
	}

	public void Cancel()
	{
		if (Status == BookingStatus.Completed)
			throw new InvalidOperationException("Completed bookings cannot be cancelled");

		Status = BookingStatus.Cancelled;
		UpdatedAt = DateTime.UtcNow;
	}

	public void Complete()
	{
		if (Status != BookingStatus.Confirmed)
			throw new InvalidOperationException("Only confirmed bookings can be completed");

		Status = BookingStatus.Completed;
		UpdatedAt = DateTime.UtcNow;
	}
}
