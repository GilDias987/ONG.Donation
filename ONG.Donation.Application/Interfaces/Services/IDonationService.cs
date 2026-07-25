using ONG.Donation.Application.DTOs;

namespace ONG.Donation.Application.Interfaces;

public interface IDonationService
{
    Task<DonationResponse> CreateAsync(int donorId, int userId, CreateDonationRequest request);
    Task<IEnumerable<DonationResponse>> GetByCampaignIdAsync(int campaignId);
}
