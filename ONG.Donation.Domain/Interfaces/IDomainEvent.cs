namespace ONG.Donation.Domain.Interfaces;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
