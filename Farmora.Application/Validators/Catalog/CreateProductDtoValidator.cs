using Farmora.Application.DTO.Catalog;
using FluentValidation;

namespace Farmora.Application.Validators.Catalog;

public sealed class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty).WithMessage("CategoryId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be >= 0.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock must be >= 0.");

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage("Invalid unit.");
    }
}
