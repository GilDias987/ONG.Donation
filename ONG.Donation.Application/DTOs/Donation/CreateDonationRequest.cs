namespace ONG.Donation.Application.DTOs;

public record CreateDonationRequest(int CampaignId, decimal Amount);
