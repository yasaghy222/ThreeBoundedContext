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
            throw new InvalidOperationException($"User with ID {request.UserId} not found");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException($"User with ID {request.UserId} is not active");
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
