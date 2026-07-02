namespace ONG.Donation.Application.Commands;

public record CreateCampaignCommand(string Title, string Description, DateTime StartDate, DateTime EndDate, decimal FinancialGoal);
