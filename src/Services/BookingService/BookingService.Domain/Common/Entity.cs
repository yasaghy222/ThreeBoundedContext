namespace BookingService.Domain.Common;

public abstract class Entity
{
	public Guid Id { get; protected set; }

	private readonly List<IDomainEvent> _domainEvents = new();
	public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

	protected Entity()
	{
		Id = Guid.NewGuid();
	}

	protected Entity(Guid id)
	{
		Id = id;
	}

	public void AddDomainEvent(IDomainEvent domainEvent)
	{
		_domainEvents.Add(domainEvent);
	}

	public void RemoveDomainEvent(IDomainEvent domainEvent)
	{
		_domainEvents.Remove(domainEvent);
	}

	public void ClearDomainEvents()
	{
		_domainEvents.Clear();
	}
}
