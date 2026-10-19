using FluentValidation;
using Farmora.Application.DTO.Reviews;

namespace Farmora.Application.Validators;

public sealed class CreateReviewValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(2000).When(x => x.Comment is not null);
    }
}
