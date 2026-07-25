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
