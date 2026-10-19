using Farmora.Application.DTO.Catalog;
using FluentValidation;

namespace Farmora.Application.Validators.Catalog;

public sealed class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty)
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Stock.HasValue);

        RuleFor(x => x.Unit)
            .IsInEnum()
            .When(x => x.Unit.HasValue);
    }
}
