using Microsoft.EntityFrameworkCore;
using ONG.Donation.Application.Interfaces;
using DomainDonation = ONG.Donation.Domain.Entities.Donation;
using ONG.Donation.Infrastructure.Persistence.Context;

namespace ONG.Donation.Infrastructure.Persistence.Repositories;

public class DonationRepository : IDonationRepository
{
    private readonly AppDbContext _context;

    public DonationRepository(AppDbContext context) => _context = context;

    public async Task<DomainDonation?> GetByIdAsync(int id) =>
        await _context.Donations.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IEnumerable<DomainDonation>> GetByCampaignIdAsync(int campaignId) =>
        await _context.Donations.Where(d => d.CampaignId == campaignId).ToListAsync();

    public async Task AddAsync(DomainDonation donation) =>
        await _context.Donations.AddAsync(donation);

    public void Update(DomainDonation donation) =>
        _context.Donations.Update(donation);
}
