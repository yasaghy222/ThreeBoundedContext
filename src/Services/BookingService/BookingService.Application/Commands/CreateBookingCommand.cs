using MediatR;
using BookingService.Application.DTOs;

namespace BookingService.Application.Commands;

public record CreateBookingCommand(
    Guid UserId,
    string Description,
    decimal Amount,
    DateTime BookingDate
) : IRequest<CreateBookingResponse>;
