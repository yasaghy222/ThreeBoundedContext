using MediatR;
using BookingService.Application.DTOs;
using BookingService.Domain.Repositories;

namespace BookingService.Application.Queries;

public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingDto?>
{
    private readonly IBookingRepository _bookingRepository;

    public GetBookingByIdQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<BookingDto?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        
        if (booking == null)
            return null;

        return new BookingDto(
            booking.Id,
            booking.UserId,
            booking.UserEmail,
            booking.UserFullName,
            booking.Description,
            booking.Amount,
            booking.BookingDate,
            booking.Status.ToString(),
            booking.CreatedAt
        );
    }
}

public class GetBookingsByUserIdQueryHandler : IRequestHandler<GetBookingsByUserIdQuery, IEnumerable<BookingDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetBookingsByUserIdQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<IEnumerable<BookingDto>> Handle(GetBookingsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        
        return bookings.Select(b => new BookingDto(
            b.Id,
            b.UserId,
            b.UserEmail,
            b.UserFullName,
            b.Description,
            b.Amount,
            b.BookingDate,
            b.Status.ToString(),
            b.CreatedAt
        ));
    }
}

public class GetAllBookingsQueryHandler : IRequestHandler<GetAllBookingsQuery, IEnumerable<BookingDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetAllBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<IEnumerable<BookingDto>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.GetAllAsync(cancellationToken);
        
        return bookings.Select(b => new BookingDto(
            b.Id,
            b.UserId,
            b.UserEmail,
            b.UserFullName,
            b.Description,
            b.Amount,
            b.BookingDate,
            b.Status.ToString(),
            b.CreatedAt
        ));
    }
}
