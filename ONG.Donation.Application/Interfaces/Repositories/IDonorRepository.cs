using ONG.Donation.Domain.Entities;

namespace ONG.Donation.Application.Interfaces;

public interface IDonorRepository
{
    Task<Donor?> GetByIdAsync(int id);
    Task<Donor?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
    Task AddAsync(Donor donor);
    Task UpdateAsync(Donor donor);
    Task DeleteAsync(Donor donor);
}
