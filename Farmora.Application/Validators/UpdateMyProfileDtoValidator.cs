using FluentValidation;
using Farmora.Application.DTO.Account;

namespace Farmora.Application.Validators;

public sealed class UpdateMyProfileDtoValidator : AbstractValidator<UpdateMyProfileDto>
{
    public UpdateMyProfileDtoValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(256)
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Email is invalid.");

        RuleFor(x => x.Phone)
            .MaximumLength(32)
            .Matches(@"^\+?[0-9\s\-()]*$")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Phone number is invalid.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(128)
            .When(x => x.DisplayName is not null);
    }
}
