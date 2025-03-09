using ap.nexus.agents.api.contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ap.nexus.agents.api.Validators
{
    public class ChatRequestValidator : Validator<ChatRequest>
    {
        public ChatRequestValidator()
        {
            // AgentId validation
            RuleFor(x => x.AgentId)
                .NotEmpty()
                .WithMessage("Agent ID is required.")
                .Must(id => id != Guid.Empty)
                .WithMessage("Agent ID cannot be an empty GUID.");

            // Message validation
            RuleFor(x => x.Message)
                .NotNull()
                .WithMessage("Message is required.");

            // Message content validation
            RuleFor(x => x.Message.Content)
                .NotEmpty()
                .WithMessage("Message content cannot be empty.")
                .When(x => x.Message != null);
            

            // ThreadId validation - if provided, it can't be an empty GUID
            RuleFor(x => x.ThreadId)
                .Must(id => !id.HasValue || id.Value != Guid.Empty)
                .WithMessage("Thread ID cannot be an empty GUID.");
        }
    }
}