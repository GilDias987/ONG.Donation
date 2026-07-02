using Microsoft.EntityFrameworkCore;
using ONG.Donation.Application.Interfaces;
using ONG.Donation.Infrastructure.Persistence.Context;

namespace ONG.Donation.Infrastructure.Persistence.Repositories;

public class CampaignRepository : ICampaignRepository
{
    private readonly AppDbContext _context;

    public CampaignRepository(AppDbContext context) => _context = context;

    public async Task<Domain.Entities.Campaign?> GetByIdAsync(int id) =>
        await _context.Campaigns.Include(c => c.Donations).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Domain.Entities.Campaign>> GetAllAsync() =>
        await _context.Campaigns.Include(c => c.Donations).ToListAsync();

    public async Task<IEnumerable<Domain.Entities.Campaign>> GetActiveAsync() =>
        await _context.Campaigns
            .Include(c => c.Donations)
            .Where(c => c.Status == Domain.Enums.CampaignStatus.Ativa)
            .ToListAsync();

    public async Task AddAsync(Domain.Entities.Campaign campaign) =>
        await _context.Campaigns.AddAsync(campaign);

    public void Update(Domain.Entities.Campaign campaign) =>
        _context.Campaigns.Update(campaign);
}
