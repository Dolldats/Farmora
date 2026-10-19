using FluentValidation;
using Farmora.Application.DTO.Reports;

namespace Farmora.Application.Validators;

public sealed class CreateReportValidator : AbstractValidator<CreateReportDto>
{
    private static readonly string[] AllowedTargets = ["Product", "Farmer", "Order", "Review", "User", "Conversation", "Message"];

    public CreateReportValidator()
    {
        RuleFor(x => x.TargetType).NotEmpty().MaximumLength(50)
            .Must(t => AllowedTargets.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"TargetType must be one of: {string.Join(", ", AllowedTargets)}.");
        RuleFor(x => x.TargetId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(5).MaximumLength(2000);
        RuleFor(x => x.EvidenceUrl).MaximumLength(2048).When(x => x.EvidenceUrl is not null);
    }
}
