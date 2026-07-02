namespace ONG.Donation.Application.DTOs;

public record DonationResponse(int Id, int CampaignId, decimal Amount, string Status);
