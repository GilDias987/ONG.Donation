using ONG.Donation.Domain.Entities;

namespace ONG.Donation.Application.Interfaces;

public interface ICampaignRepository
{
    Task<Campaign?> GetByIdAsync(int id);
    Task<IEnumerable<Campaign>> GetAllAsync();
    Task<IEnumerable<Campaign>> GetActiveAsync();
    Task AddAsync(Campaign campaign);
    void Update(Campaign campaign);
}
