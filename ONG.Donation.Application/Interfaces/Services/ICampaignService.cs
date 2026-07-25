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
    Task<CampaignResponse> UpdateValueAsync(int id, UpdateCampaignValueRequest request);
}
