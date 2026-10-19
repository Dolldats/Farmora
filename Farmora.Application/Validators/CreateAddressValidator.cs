using FluentValidation;
using Farmora.Application.DTO.Address;

namespace Farmora.Application.Validators;

public sealed class CreateAddressValidator : AbstractValidator<CreateAddressDto>
{
    public CreateAddressValidator()
    {
        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Line1).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Line2).MaximumLength(500).When(x => x.Line2 is not null);
        RuleFor(x => x.City).NotEmpty().MaximumLength(200);
        RuleFor(x => x.State).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PostalCode).MaximumLength(20).When(x => x.PostalCode is not null);
        RuleFor(x => x.Phone).MaximumLength(30).When(x => x.Phone is not null);
    }
}
