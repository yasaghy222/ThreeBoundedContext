namespace UserService.Domain.Common;

public interface IDomainEventDispatcher
{
	Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
