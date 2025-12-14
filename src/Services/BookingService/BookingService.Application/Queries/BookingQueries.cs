using MediatR;
using BookingService.Application.DTOs;

namespace BookingService.Application.Queries;

public record GetBookingByIdQuery(Guid BookingId) : IRequest<BookingDto?>;

public record GetBookingsByUserIdQuery(Guid UserId) : IRequest<IEnumerable<BookingDto>>;

public record GetAllBookingsQuery : IRequest<IEnumerable<BookingDto>>;
