using ONG.Donation.Domain.Interfaces;

namespace ONG.Donation.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T domainEvent) where T : IDomainEvent;
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
