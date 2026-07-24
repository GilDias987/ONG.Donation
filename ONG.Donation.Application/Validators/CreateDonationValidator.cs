using FluentValidation;
using ONG.Donation.Application.Commands;

namespace ONG.Donation.Application.Validators;

public class CreateDonationValidator : AbstractValidator<CreateDonationCommand>
{
    public CreateDonationValidator()
    {
        RuleFor(x => x.CampaignId).GreaterThan(0).WithMessage("Campaign is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}
