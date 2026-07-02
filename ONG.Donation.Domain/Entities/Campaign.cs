using ONG.Donation.Domain.Common;
using ONG.Donation.Domain.Enums;

namespace ONG.Donation.Domain.Entities;

public class Campaign : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal FinancialGoal { get; private set; }
    public CampaignStatus Status { get; private set; }
    public ICollection<Donation> Donations { get; private set; } = [];

    private Campaign() { }

    public Campaign(string title, string description, DateTime startDate, DateTime endDate, decimal financialGoal)
    {
        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        FinancialGoal = financialGoal;
        Status = CampaignStatus.Ativa;
        Validate();
    }

    public void Update(string title, string description, DateTime startDate, DateTime endDate, decimal financialGoal)
    {
        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        FinancialGoal = financialGoal;
        Validate();
        SetUpdatedAt();
    }

    public void Activate()
    {
        if (Status != CampaignStatus.Cancelada)
            throw new Exceptions.DomainException("Only cancelled campaigns can be activated.");

        Status = CampaignStatus.Ativa;
        SetUpdatedAt();
    }

    public void Inactivate()
    {
        if (Status != CampaignStatus.Ativa)
            throw new Exceptions.DomainException("Only active campaigns can be inactivated.");

        Status = CampaignStatus.Cancelada;
        SetUpdatedAt();
    }

    public void Complete()
    {
        Status = CampaignStatus.Concluida;
        SetUpdatedAt();
    }

    public void Cancel()
    {
        Status = CampaignStatus.Cancelada;
        SetUpdatedAt();
    }

    public bool IsWithinPeriod() =>
        DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;

    public decimal GetTotalRaised() => Donations
        .Where(d => d.Status == DonationStatus.Processada)
        .Sum(d => d.Amount);

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Title))
            throw new Exceptions.DomainException("Title is required.");

        if (EndDate <= DateTime.UtcNow)
            throw new Exceptions.DomainException("End date cannot be in the past.");

        if (FinancialGoal <= 0)
            throw new Exceptions.DomainException("Financial goal must be greater than zero.");
    }
}
