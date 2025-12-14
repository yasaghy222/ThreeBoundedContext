using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using BookingService.Domain.Common;
using BookingService.Domain.Entities;
using BookingService.Domain.Events;
using BookingService.Domain.Repositories;
using BookingService.Infrastructure.Outbox;
using Shared.Contracts.Events;

namespace BookingService.Infrastructure.Persistence;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;

    public BookingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Add booking
            await _context.Bookings.AddAsync(booking, cancellationToken);

            // Add domain events to outbox (Outbox Pattern)
            foreach (var domainEvent in booking.DomainEvents)
            {
                if (domainEvent is BookingCreatedDomainEvent bookingCreated)
                {
                    var integrationEvent = new BookingCreatedEvent(
                        bookingCreated.BookingId,
                        bookingCreated.UserId,
                        bookingCreated.UserEmail,
                        bookingCreated.UserFullName,
                        bookingCreated.Description,
                        bookingCreated.Amount,
                        bookingCreated.BookingDate,
                        bookingCreated.CreatedAt
                    );

                    var outboxMessage = new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        Type = nameof(BookingCreatedEvent),
                        Content = JsonSerializer.Serialize(integrationEvent),
                        OccurredAt = DateTime.UtcNow
                    };

                    await _context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            booking.ClearDomainEvents();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
