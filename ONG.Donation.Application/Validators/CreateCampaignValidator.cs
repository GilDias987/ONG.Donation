using FluentValidation;
using ONG.Donation.Application.Commands;

namespace ONG.Donation.Application.Validators;

public class CreateCampaignValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
        RuleFor(x => x.StartDate).NotEmpty().WithMessage("Start date is required.");
        RuleFor(x => x.EndDate).NotEmpty().WithMessage("End date is required.");
        RuleFor(x => x.FinancialGoal).GreaterThan(0).WithMessage("Financial goal must be greater than zero.");
    }
}
