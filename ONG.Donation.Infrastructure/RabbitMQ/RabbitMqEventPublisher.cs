using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ONG.Donation.Application.Interfaces;
using ONG.Donation.Domain.Interfaces;
using RabbitMQ.Client;

namespace ONG.Donation.Infrastructure.RabbitMQ;

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private const string ExchangeName = "donation.events";

    public RabbitMqEventPublisher(ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "owng",
            Password = "owong"
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true).GetAwaiter().GetResult();

        _logger.LogInformation("RabbitMQ connection established, exchange {ExchangeName} declared", ExchangeName);
    }

    public async Task PublishAsync<T>(T domainEvent) where T : IDomainEvent
    {
        var routingKey = typeof(T).Name;
        var body = JsonSerializer.SerializeToUtf8Bytes(domainEvent);

        await _channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: routingKey,
            body: body);

        _logger.LogInformation("Event {EventType} published to {Exchange}/{RoutingKey}",
            routingKey, ExchangeName, routingKey);
    }

    public void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _connection?.CloseAsync().GetAwaiter().GetResult();
        _logger.LogInformation("RabbitMQ connection closed");
    }
}
