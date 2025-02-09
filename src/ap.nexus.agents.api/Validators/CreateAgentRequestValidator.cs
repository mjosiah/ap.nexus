using ap.nexus.abstractions.Agents.DTOs;
using FastEndpoints;
using FluentValidation;

namespace ap.nexus.agents.api.Validators
{
    public class CreateAgentRequestValidator : Validator<CreateAgentRequest>
    {
        public CreateAgentRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Agent name is required.");

            RuleFor(x => x.Model)
                .NotEmpty()
                .WithMessage("Agent model is required.");
        }
    }
}
    
