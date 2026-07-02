using ONG.Donation.Domain.Interfaces;

namespace ONG.Donation.Domain.Events;

public record DonationPaymentProcessedEvent(
    int DonationId,
    DateTime OccurredAt) : IDomainEvent;
