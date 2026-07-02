namespace ONG.Donation.Application.Commands;

public record CreateDonationCommand(int CampaignId, decimal Amount);
