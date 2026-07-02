using FluentValidation;
using ONG.Donation.Application.Commands;

namespace ONG.Donation.Application.Validators;

public class RegisterDonorValidator : AbstractValidator<RegisterDonorCommand>
{
    public RegisterDonorValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Full name is required.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid email is required.");
        RuleFor(x => x.Cpf).NotEmpty().WithMessage("CPF is required.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}

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

public class UpdateCampaignValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
        RuleFor(x => x.FinancialGoal).GreaterThan(0).WithMessage("Financial goal must be greater than zero.");
    }
}

public class CreateDonationValidator : AbstractValidator<CreateDonationCommand>
{
    public CreateDonationValidator()
    {
        RuleFor(x => x.CampaignId).GreaterThan(0).WithMessage("Campaign is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}
