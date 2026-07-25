using FluentValidation;
using ONG.Donation.Application.Commands;

namespace ONG.Donation.Application.Validators;

public class UpdateCampaignValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
        RuleFor(x => x.FinancialGoal).GreaterThan(0).WithMessage("Financial goal must be greater than zero.");
    }
}
