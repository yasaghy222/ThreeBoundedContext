using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;
using FinanceService.Application.Commands;

namespace FinanceService.Infrastructure.Messaging;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
}

public class BookingCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingCreatedConsumer> _logger;
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string ExchangeName = "booking-events";
    private const string QueueName = "finance-booking-created";
    private const string RoutingKey = "booking.created";

    public BookingCreatedConsumer(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingCreatedConsumer> logger,
        IOptions<RabbitMqSettings> settings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BookingCreated Consumer starting...");

        await InitializeAsync(stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel!);
        
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                _logger.LogInformation("Received BookingCreated message: {Message}", message);
                
                var bookingCreatedEvent = JsonSerializer.Deserialize<BookingCreatedEvent>(message);
                
                if (bookingCreatedEvent != null)
                {
                    await ProcessBookingCreatedAsync(bookingCreatedEvent, stoppingToken);
                }

                await _channel!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing BookingCreated message");
                // Nack the message without requeue to avoid infinite loop
                // In production, you might want to implement a dead letter queue
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            }
        };

        await _channel!.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("BookingCreated Consumer started, waiting for messages...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: RoutingKey,
            cancellationToken: cancellationToken);

        await _channel.BasicQosAsync(0, 1, false, cancellationToken);
    }

    private async Task ProcessBookingCreatedAsync(BookingCreatedEvent bookingCreated, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new CreateInvoiceCommand(
            bookingCreated.BookingId,
            bookingCreated.UserId,
            bookingCreated.UserEmail,
            bookingCreated.UserFullName,
            bookingCreated.Description,
            bookingCreated.Amount,
            bookingCreated.BookingDate
        );

        var result = await mediator.Send(command, cancellationToken);
        
        _logger.LogInformation(
            "Created invoice {InvoiceId} ({InvoiceNumber}) for booking {BookingId}",
            result.InvoiceId,
            result.InvoiceNumber,
            bookingCreated.BookingId);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BookingCreated Consumer stopping...");

        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
            await _channel.DisposeAsync();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync(cancellationToken);
            await _connection.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}
