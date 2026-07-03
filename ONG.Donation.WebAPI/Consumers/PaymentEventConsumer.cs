using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ONG.Donation.Application.Interfaces;
using ONG.Donation.Domain.Enums;
using ONG.Donation.Domain.Events;
using ONG.Donation.Infrastructure.Persistence.Context;
using ONG.Donation.Infrastructure.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ONG.Donation.WebAPI.Consumers;

public class PaymentEventConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentEventConsumer> _logger;
    private readonly RabbitMQOptions _rabbitMqOptions;
    private IConnection _connection = null!;
    private IChannel _channel = null!;

    public PaymentEventConsumer(IServiceProvider serviceProvider, ILogger<PaymentEventConsumer> logger, IOptions<RabbitMQOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMqOptions = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqOptions.HostName,
            UserName = _rabbitMqOptions.UserName,
            Password = _rabbitMqOptions.Password
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(null, stoppingToken);

        await _channel.ExchangeDeclareAsync(_rabbitMqOptions.ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

        var queueName = "donation.payment.result";
        await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(queueName, _rabbitMqOptions.ExchangeName, "DonationPaymentProcessedEvent", cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(queueName, _rabbitMqOptions.ExchangeName, "DonationPaymentFailedEvent", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var routingKey = ea.RoutingKey;
                _logger.LogInformation("Payment event received: {RoutingKey}", routingKey);

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var donationRepository = scope.ServiceProvider.GetRequiredService<IDonationRepository>();

                if (routingKey == "DonationPaymentProcessedEvent")
                {
                    var paymentEvent = JsonSerializer.Deserialize<DonationPaymentProcessedEvent>(body);
                    if (paymentEvent is null)
                    {
                        _logger.LogWarning("Failed to deserialize DonationPaymentProcessedEvent.");
                        return;
                    }

                    var donation = await donationRepository.GetByIdAsync(paymentEvent.DonationId);
                    if (donation is null)
                    {
                        _logger.LogWarning("Donation {Id} not found.", paymentEvent.DonationId);
                        return;
                    }

                    donation.MarkAsProcessed();
                    donationRepository.Update(donation);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Donation {Id} marked as processed.", donation.Id);
                }
                else if (routingKey == "DonationPaymentFailedEvent")
                {
                    var paymentEvent = JsonSerializer.Deserialize<DonationPaymentFailedEvent>(body);
                    if (paymentEvent is null)
                    {
                        _logger.LogWarning("Failed to deserialize DonationPaymentFailedEvent.");
                        return;
                    }

                    var donation = await donationRepository.GetByIdAsync(paymentEvent.DonationId);
                    if (donation is null)
                    {
                        _logger.LogWarning("Donation {Id} not found.", paymentEvent.DonationId);
                        return;
                    }

                    donation.MarkAsFailed();
                    donationRepository.Update(donation);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Donation {Id} marked as failed.", donation.Id);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment event.");
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        _logger.LogInformation("PaymentEventConsumer started, waiting for payment result events...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("PaymentEventConsumer stopping...");
        if (_channel is not null) await _channel.CloseAsync(cancellationToken);
        if (_connection is not null) await _connection.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
