namespace ONG.Donation.Application.DTOs;

public record CampaignResponse(int Id, string Title, string Description, DateTime StartDate, DateTime EndDate, decimal FinancialGoal, string Status, decimal TotalRaised);
