using ONG.Donation.Application.DTOs;

namespace ONG.Donation.Application.Interfaces;

public interface IDonorService
{
    Task<int> RegisterAsync(RegisterDonorRequest request);
    Task<DonorResponse> GetByIdAsync(int id);
    Task<DonorResponse> UpdateAsync(int id, UpdateDonorRequest request);
    Task DeleteAsync(int id);
}
