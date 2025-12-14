namespace Shared.Infrastructure.Messaging;

public interface IMessagePublisher
{
	Task PublishAsync<T>(T message, string exchangeName, string routingKey, CancellationToken cancellationToken = default) where T : class;
}
