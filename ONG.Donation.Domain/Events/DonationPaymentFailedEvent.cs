using ONG.Donation.Domain.Interfaces;

namespace ONG.Donation.Domain.Events;

public record DonationPaymentFailedEvent(
    int DonationId,
    string Reason,
    DateTime OccurredAt) : IDomainEvent;
