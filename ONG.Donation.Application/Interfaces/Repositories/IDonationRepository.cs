namespace ONG.Donation.Application.Interfaces;

public interface IDonationRepository
{
    Task<global::ONG.Donation.Domain.Entities.Donation?> GetByIdAsync(int id);
    Task<IEnumerable<global::ONG.Donation.Domain.Entities.Donation>> GetByCampaignIdAsync(int campaignId);
    Task AddAsync(global::ONG.Donation.Domain.Entities.Donation donation);
    void Update(global::ONG.Donation.Domain.Entities.Donation donation);
}
