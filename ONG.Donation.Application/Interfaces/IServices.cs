using ONG.Donation.Application.DTOs;

namespace ONG.Donation.Application.Interfaces;

public interface ICampaignService
{
    Task<IEnumerable<CampaignResponse>> GetAllAsync();
    Task<IEnumerable<TransparencyCampaignResponse>> GetActiveAsync();
    Task<CampaignResponse> GetByIdAsync(int id);
    Task<CampaignResponse> CreateAsync(CreateCampaignRequest request);
    Task<CampaignResponse> UpdateAsync(int id, UpdateCampaignRequest request);
    Task<CampaignResponse> SetStatusAsync(int id, SetCampaignStatusRequest request);
}

public interface IDonationService
{
    Task<DonationResponse> CreateAsync(int donorId, int userId, CreateDonationRequest request);
    Task<IEnumerable<DonationResponse>> GetByCampaignIdAsync(int campaignId);
}

public interface IDonorService
{
    Task<int> RegisterAsync(RegisterDonorRequest request);
    Task<DonorResponse> GetByIdAsync(int id);
    Task<DonorResponse> UpdateAsync(int id, UpdateDonorRequest request);
    Task DeleteAsync(int id);
}
