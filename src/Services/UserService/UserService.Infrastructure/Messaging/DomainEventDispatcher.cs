using Shared.Contracts.Events;
using Shared.Infrastructure.Messaging;
using UserService.Domain.Common;
using UserService.Domain.Events;

namespace UserService.Infrastructure.Messaging;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMessagePublisher _messagePublisher;
    private const string ExchangeName = "user-events";

    public DomainEventDispatcher(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await DispatchEventAsync(domainEvent, cancellationToken);
        }
    }

    private async Task DispatchEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        switch (domainEvent)
        {
            case UserCreatedDomainEvent userCreated:
                var integrationEvent = new UserCreatedEvent(
                    userCreated.UserId,
                    userCreated.Email,
                    userCreated.FullName,
                    userCreated.CreatedAt
                );
                await _messagePublisher.PublishAsync(
                    integrationEvent,
                    ExchangeName,
                    "user.created",
                    cancellationToken);
                break;
        }
    }
}
