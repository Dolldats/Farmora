using Farmora.Application.DTO.Catalog;
using FluentValidation;

namespace Farmora.Application.Validators.Catalog;

public sealed class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Slug)
            .Matches("^[a-z0-9-]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.Slug))
            .WithMessage("Slug may only contain lowercase letters, numbers and hyphens.")
            .MaximumLength(100);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ParentCategoryId)
            .NotEqual(Guid.Empty)
            .When(x => x.ParentCategoryId.HasValue);
    }
}
