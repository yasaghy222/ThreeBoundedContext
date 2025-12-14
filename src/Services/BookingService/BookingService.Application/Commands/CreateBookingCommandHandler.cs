using ErrorHandling.Core.Exceptions;
using MediatR;
using BookingService.Application.DTOs;
using BookingService.Domain.Entities;
using BookingService.Domain.Repositories;
using BookingService.Domain.Services;

namespace BookingService.Application.Commands;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, CreateBookingResponse>
{
	private readonly IBookingRepository _bookingRepository;
	private readonly IUserValidationService _userValidationService;

	public CreateBookingCommandHandler(
	    IBookingRepository bookingRepository,
	    IUserValidationService userValidationService)
	{
		_bookingRepository = bookingRepository;
		_userValidationService = userValidationService;
	}

	public async Task<CreateBookingResponse> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
	{
		// Validate user exists and is active via gRPC (sync call)
		var user = await _userValidationService.GetUserAsync(request.UserId, cancellationToken);

		if (user == null)
		{
			throw new NotFoundException($"User with ID {request.UserId} not found", "user_not_found");
		}

		if (!user.IsActive)
		{
			throw new BadRequestException($"User with ID {request.UserId} is not active", "user_not_active");
		}

		// Create booking entity
		var booking = Booking.Create(
		    user.UserId,
		    user.Email,
		    user.FullName,
		    request.Description,
		    request.Amount,
		    request.BookingDate
		);

		// Save booking (this also saves to Outbox table in same transaction)
		await _bookingRepository.AddAsync(booking, cancellationToken);

		return new CreateBookingResponse(
		    booking.Id,
		    booking.UserId,
		    booking.Description,
		    booking.Amount,
		    booking.BookingDate,
		    booking.Status.ToString()
		);
	}
}
