using FluentValidation;
using Farmora.Application.DTO.Cart;

namespace Farmora.Application.Validators;

public sealed class AddCartItemValidator : AbstractValidator<AddCartItemDto>
{
    public AddCartItemValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
