using FluentValidation;
using Farmora.Application.DTO.Auth;

namespace Farmora.Application.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Phone)
            .MaximumLength(32)
            .Matches(@"^\+?[0-9\s\-()]*$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Phone number is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches(@"[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain a digit.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => r.Equals("Customer", StringComparison.OrdinalIgnoreCase) ||
                       r.Equals("Farmer", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Role must be Customer or Farmer.");
    }
}
