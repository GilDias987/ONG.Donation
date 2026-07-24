using FluentValidation;
using ONG.Donation.Application.Commands;

namespace ONG.Donation.Application.Validators;

public class UpdateCampaignValueValidator : AbstractValidator<UpdateCampaignValueCommand>
{
    public UpdateCampaignValueValidator()
    {
        RuleFor(x => x.FinancialGoal).GreaterThan(0).WithMessage("Financial goal must be greater than zero.");
    }
}
