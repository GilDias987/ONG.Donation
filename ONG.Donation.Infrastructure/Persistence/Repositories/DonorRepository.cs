using Microsoft.EntityFrameworkCore;
using ONG.Donation.Application.Interfaces;
using DomainDonor = ONG.Donation.Domain.Entities.Donor;
using ONG.Donation.Infrastructure.Persistence.Context;

namespace ONG.Donation.Infrastructure.Persistence.Repositories;

public class DonorRepository : IDonorRepository
{
    private readonly AppDbContext _context;

    public DonorRepository(AppDbContext context) => _context = context;

    public async Task<DomainDonor?> GetByIdAsync(int id) =>
        await _context.Donors.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<DomainDonor?> GetByEmailAsync(string email) =>
        await _context.Donors.FirstOrDefaultAsync(d => d.Email == email);

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        if (excludeId.HasValue)
            return await _context.Donors.AnyAsync(d => d.Email == email && d.Id != excludeId.Value);
        return await _context.Donors.AnyAsync(d => d.Email == email);
    }

    public async Task AddAsync(DomainDonor donor) =>
        await _context.Donors.AddAsync(donor);

    public Task UpdateAsync(DomainDonor donor)
    {
        _context.Donors.Update(donor);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(DomainDonor donor)
    {
        _context.Donors.Remove(donor);
        return Task.CompletedTask;
    }
}
