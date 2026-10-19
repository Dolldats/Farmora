using FluentValidation;
using Farmora.Application.DTO.Messaging;

namespace Farmora.Application.Validators;

public sealed class SendMessageValidator : AbstractValidator<SendMessageDto>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MinimumLength(1).MaximumLength(5000);
    }
}

public sealed class CreateConversationValidator : AbstractValidator<CreateConversationDto>
{
    public CreateConversationValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FirstMessage).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.ParticipantIds).NotNull().Must(p => p.Count >= 1 && p.Count <= 20)
            .WithMessage("ParticipantIds must contain between 1 and 20 participants.");
    }
}
