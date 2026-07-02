using ONG.Donation.Domain.Common;
using ONG.Donation.Domain.Enums;

namespace ONG.Donation.Domain.Entities;

public class Donation : BaseEntity
{
    public int CampaignId { get; private set; }
    public Campaign Campaign { get; private set; }
    public int DonorId { get; private set; }
    public Donor Donor { get; private set; }
    public decimal Amount { get; private set; }
    public DonationStatus Status { get; private set; }

    private Donation() { }

    public Donation(int campaignId, int donorId, decimal amount)
    {
        CampaignId = campaignId;
        DonorId = donorId;
        Amount = amount;
        Status = DonationStatus.Pendente;
        Validate();
    }

    public void MarkAsProcessed()
    {
        Status = DonationStatus.Processada;
        SetUpdatedAt();
    }

    public void MarkAsFailed()
    {
        Status = DonationStatus.Falhou;
        SetUpdatedAt();
    }

    private void Validate()
    {
        if (Amount <= 0)
            throw new Exceptions.DomainException("Donation amount must be greater than zero.");
    }
}
