namespace ONG.Donation.Application.Commands;

public record UpdateCampaignCommand(int Id, string Title, string Description, DateTime StartDate, DateTime EndDate, decimal FinancialGoal);
