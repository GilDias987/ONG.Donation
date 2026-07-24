using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ONG.Donation.Application.Interfaces;
using ONG.Donation.Domain.Events;
using ONG.Donation.Infrastructure.ServiceBus;

namespace ONG.Donation.Worker.Consumers;

public class DonationConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DonationConsumer> _logger;
    private readonly ServiceBusOptions _serviceBusOptions;
    private ServiceBusProcessor? _processor;

    public DonationConsumer(
        IServiceProvider serviceProvider,
        ILogger<DonationConsumer> logger,
        IOptions<ServiceBusOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _serviceBusOptions = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var client = new ServiceBusClient(_serviceBusOptions.ConnectionString);
        _processor = client.CreateProcessor(
            _serviceBusOptions.QueueName,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });

        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("DonationConsumer started, waiting for messages...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToArray();
            _logger.LogInformation("Donation event received: {Message}", args.Message.Body.ToString());

            using var scope = _serviceProvider.CreateScope();
            var donationRepository = scope.ServiceProvider.GetRequiredService<IDonationRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            var donationEvent = JsonSerializer.Deserialize<DonationCreatedEvent>(body);
            if (donationEvent is null)
            {
                _logger.LogWarning("Failed to deserialize donation event.");
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            var donation = await donationRepository.GetByIdAsync(donationEvent.DonationId);
            if (donation is null)
            {
                _logger.LogWarning("Donation {Id} not found.", donationEvent.DonationId);
                await args.CompleteMessageAsync(args.Message);
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

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing donation event.");
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus processor error: {Source}", args.ErrorSource);
        return Task.CompletedTask;
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
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
        await base.StopAsync(cancellationToken);
    }
}
