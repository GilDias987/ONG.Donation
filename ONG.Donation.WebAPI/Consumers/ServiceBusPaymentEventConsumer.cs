using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ONG.Donation.Application.Interfaces;
using ONG.Donation.Domain.Enums;
using ONG.Donation.Domain.Events;
using ONG.Donation.Infrastructure.Persistence.Context;
using ONG.Donation.Infrastructure.ServiceBus;

namespace ONG.Donation.WebAPI.Consumers;

public class ServiceBusPaymentEventConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceBusPaymentEventConsumer> _logger;
    private readonly ServiceBusOptions _serviceBusOptions;
    private ServiceBusProcessor _processor = null!;
    private ServiceBusClient _client = null!;

    public ServiceBusPaymentEventConsumer(IServiceProvider serviceProvider, ILogger<ServiceBusPaymentEventConsumer> logger, IOptions<ServiceBusOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _serviceBusOptions = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client = new ServiceBusClient(_serviceBusOptions.ConnectionString);

        _processor = _client.CreateProcessor(_serviceBusOptions.ResultQueueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false
        });

        _processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var body = args.Message.Body.ToString();
                var subject = args.Message.Subject;
                _logger.LogInformation("Payment event received: {Subject}", subject);

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var donationRepository = scope.ServiceProvider.GetRequiredService<IDonationRepository>();

                if (subject == "DonationPaymentProcessedEvent")
                {
                    var paymentEvent = JsonSerializer.Deserialize<DonationPaymentProcessedEvent>(body);
                    if (paymentEvent is null)
                    {
                        _logger.LogWarning("Failed to deserialize DonationPaymentProcessedEvent.");
                        await args.CompleteMessageAsync(args.Message, stoppingToken);
                        return;
                    }

                    var donation = await donationRepository.GetByIdAsync(paymentEvent.DonationId);
                    if (donation is null)
                    {
                        _logger.LogWarning("Donation {Id} not found.", paymentEvent.DonationId);
                        await args.CompleteMessageAsync(args.Message, stoppingToken);
                        return;
                    }

                    donation.MarkAsProcessed();
                    donationRepository.Update(donation);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Donation {Id} marked as processed.", donation.Id);
                }
                else if (subject == "DonationPaymentFailedEvent")
                {
                    var paymentEvent = JsonSerializer.Deserialize<DonationPaymentFailedEvent>(body);
                    if (paymentEvent is null)
                    {
                        _logger.LogWarning("Failed to deserialize DonationPaymentFailedEvent.");
                        await args.CompleteMessageAsync(args.Message, stoppingToken);
                        return;
                    }

                    var donation = await donationRepository.GetByIdAsync(paymentEvent.DonationId);
                    if (donation is null)
                    {
                        _logger.LogWarning("Donation {Id} not found.", paymentEvent.DonationId);
                        await args.CompleteMessageAsync(args.Message, stoppingToken);
                        return;
                    }

                    donation.MarkAsFailed();
                    donationRepository.Update(donation);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Donation {Id} marked as failed.", donation.Id);
                }

                await args.CompleteMessageAsync(args.Message, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment event.");
                await args.AbandonMessageAsync(args.Message, cancellationToken: stoppingToken);
            }
        };

        _processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception, "ServiceBus processor error");
            return Task.CompletedTask;
        };

        await _processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("ServiceBusPaymentEventConsumer started, waiting for payment result events...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ServiceBusPaymentEventConsumer stopping...");
        if (_processor is not null) await _processor.CloseAsync(cancellationToken);
        if (_client is not null) await _client.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
