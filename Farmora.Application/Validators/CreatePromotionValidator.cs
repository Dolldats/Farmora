using FluentValidation;
using Farmora.Application.DTO.Promotions;

namespace Farmora.Application.Validators;

public sealed class CreatePromotionValidator : AbstractValidator<CreatePromotionDto>
{
    public CreatePromotionValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Value).GreaterThan(0);
        RuleFor(x => x.EndsAtUtc).GreaterThan(x => x.StartsAtUtc);
        RuleFor(x => x.MaxUses).GreaterThan(0).When(x => x.MaxUses.HasValue);
        RuleFor(x => x.MinOrderAmount).GreaterThanOrEqualTo(0).When(x => x.MinOrderAmount.HasValue);
    }
}
