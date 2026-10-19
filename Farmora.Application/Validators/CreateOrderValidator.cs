using FluentValidation;
using Farmora.Application.DTO.Orders;

namespace Farmora.Application.Validators;

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PromoCode).MaximumLength(50).When(x => x.PromoCode is not null);
        RuleFor(x => x.DeliveryMethod).IsInEnum();
    }
}
