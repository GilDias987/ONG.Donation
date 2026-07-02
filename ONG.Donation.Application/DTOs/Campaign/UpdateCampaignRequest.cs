namespace ONG.Donation.Application.DTOs;

public record UpdateCampaignRequest(string Title, string Description, DateTime StartDate, DateTime EndDate, decimal FinancialGoal);
