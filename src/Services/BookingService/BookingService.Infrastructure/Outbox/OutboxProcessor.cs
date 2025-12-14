using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BookingService.Infrastructure.Persistence;
using Shared.Contracts.Events;
using Shared.Infrastructure.Messaging;

namespace BookingService.Infrastructure.Outbox;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < 3)
            .OrderBy(m => m.OccurredAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await PublishMessageAsync(message, messagePublisher, cancellationToken);
                
                message.ProcessedAt = DateTime.UtcNow;
                _logger.LogInformation("Successfully published outbox message {MessageId} of type {MessageType}", 
                    message.Id, message.Type);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                _logger.LogWarning(ex, "Failed to publish outbox message {MessageId}, retry {RetryCount}", 
                    message.Id, message.RetryCount);
            }
        }

        if (messages.Any())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task PublishMessageAsync(
        OutboxMessage message, 
        IMessagePublisher publisher,
        CancellationToken cancellationToken)
    {
        switch (message.Type)
        {
            case nameof(BookingCreatedEvent):
                var bookingCreatedEvent = JsonSerializer.Deserialize<BookingCreatedEvent>(message.Content);
                if (bookingCreatedEvent != null)
                {
                    await publisher.PublishAsync(
                        bookingCreatedEvent,
                        "booking-events",
                        "booking.created",
                        cancellationToken);
                }
                break;

            default:
                _logger.LogWarning("Unknown message type: {MessageType}", message.Type);
                break;
        }
    }
}
