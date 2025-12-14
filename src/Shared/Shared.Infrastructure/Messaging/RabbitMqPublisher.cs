using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Shared.Infrastructure.Messaging;

public class RabbitMqSettings
{
	public string HostName { get; set; } = "localhost";
	public int Port { get; set; } = 5672;
	public string UserName { get; set; } = "guest";
	public string Password { get; set; } = "guest";
	public string VirtualHost { get; set; } = "/";
}

public class RabbitMqPublisher(IOptions<RabbitMqSettings> settings) : IMessagePublisher, IAsyncDisposable
{
	private readonly RabbitMqSettings _settings = settings.Value;
	private IConnection? _connection;
	private IChannel? _channel;
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	private async Task EnsureConnectionAsync()
	{
		if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
			return;

		await _semaphore.WaitAsync();
		try
		{
			if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
				return;

			var factory = new ConnectionFactory
			{
				HostName = _settings.HostName,
				Port = _settings.Port,
				UserName = _settings.UserName,
				Password = _settings.Password,
				VirtualHost = _settings.VirtualHost
			};

			_connection = await factory.CreateConnectionAsync();
			_channel = await _connection.CreateChannelAsync();
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public async Task PublishAsync<T>(T message, string exchangeName, string routingKey, CancellationToken cancellationToken = default) where T : class
	{
		await EnsureConnectionAsync();

		await _channel!.ExchangeDeclareAsync(
		    exchange: exchangeName,
		    type: ExchangeType.Topic,
		    durable: true,
		    autoDelete: false,
		    cancellationToken: cancellationToken);

		var json = JsonSerializer.Serialize(message);
		var body = Encoding.UTF8.GetBytes(json);

		var properties = new BasicProperties
		{
			Persistent = true,
			ContentType = "application/json",
			MessageId = Guid.NewGuid().ToString(),
			Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
		};

		await _channel.BasicPublishAsync(
		    exchange: exchangeName,
		    routingKey: routingKey,
		    mandatory: false,
		    basicProperties: properties,
		    body: body,
		    cancellationToken: cancellationToken);
	}

	public async ValueTask DisposeAsync()
	{
		if (_channel is not null)
		{
			await _channel.CloseAsync();
			await _channel.DisposeAsync();
		}

		if (_connection is not null)
		{
			await _connection.CloseAsync();
			await _connection.DisposeAsync();
		}

		_semaphore.Dispose();
		GC.SuppressFinalize(this);
	}
}
