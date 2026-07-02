namespace ONG.Donation.Application.DTOs;

public record CreateCampaignRequest(string Title, string Description, DateTime StartDate, DateTime EndDate, decimal FinancialGoal);
