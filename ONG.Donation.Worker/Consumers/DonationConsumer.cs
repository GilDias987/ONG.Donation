using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ONG.Donation.Application.Interfaces;
using ONG.Donation.Domain.Enums;
using ONG.Donation.Domain.Events;
using ONG.Donation.Infrastructure.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ONG.Donation.Worker.Consumers;

public class DonationConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DonationConsumer> _logger;
    private readonly RabbitMQOptions _rabbitMqOptions;
    private IConnection _connection;
    private IChannel _channel;

    public DonationConsumer(IServiceProvider serviceProvider, ILogger<DonationConsumer> logger, IOptions<RabbitMQOptions> options)
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
        await _channel.QueueDeclareAsync(_rabbitMqOptions.QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(_rabbitMqOptions.QueueName, _rabbitMqOptions.ExchangeName, "DonationCreatedEvent", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Donation event received: {Message}", message);

                using var scope = _serviceProvider.CreateScope();
                var donationRepository = scope.ServiceProvider.GetRequiredService<IDonationRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                var donationEvent = JsonSerializer.Deserialize<DonationCreatedEvent>(body);
                if (donationEvent is null)
                {
                    _logger.LogWarning("Failed to deserialize donation event.");
                    return;
                }

                var donation = await donationRepository.GetByIdAsync(donationEvent.DonationId);
                if (donation is null)
                {
                    _logger.LogWarning("Donation {Id} not found.", donationEvent.DonationId);
                    return;
                }

                var success = await ProcessPaymentAsync(donation);
                if (success)
                {
                    donation.MarkAsProcessed();
                    donationRepository.Update(donation);
                    await unitOfWork.SaveChangesAsync();
                    await eventPublisher.PublishAsync(new DonationPaymentProcessedEvent(
                        donation.Id, DateTime.UtcNow));
                    _logger.LogInformation("Donation {Id} processed successfully.", donation.Id);
                }
                else
                {
                    donation.MarkAsFailed();
                    donationRepository.Update(donation);
                    await unitOfWork.SaveChangesAsync();
                    await eventPublisher.PublishAsync(new DonationPaymentFailedEvent(
                        donation.Id, "Payment processing failed.", DateTime.UtcNow));
                    _logger.LogWarning("Donation {Id} processing failed.", donation.Id);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing donation event.");
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(_rabbitMqOptions.QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        _logger.LogInformation("DonationConsumer started, waiting for messages...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private static Task<bool> ProcessPaymentAsync(global::ONG.Donation.Domain.Entities.Donation donation)
    {
        try
        {
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DonationConsumer stopping...");
        if (_channel is not null) await _channel.CloseAsync(cancellationToken);
        if (_connection is not null) await _connection.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
