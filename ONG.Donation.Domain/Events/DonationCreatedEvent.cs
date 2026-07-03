using ONG.Donation.Domain.Interfaces;

namespace ONG.Donation.Domain.Events;

public record DonationCreatedEvent(
    int DonationId,
    int CampaignId,
    int DonorId,
    int UserId,
    decimal Amount,
    DateTime OccurredAt) : IDomainEvent;
