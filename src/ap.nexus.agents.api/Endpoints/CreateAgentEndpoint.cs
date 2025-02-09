using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.application.Exceptions;
using FastEndpoints;
using System.ComponentModel.DataAnnotations;

namespace ap.nexus.agents.api.Endpoints
{
    public class CreateAgentEndpoint : Endpoint<CreateAgentRequest, AgentDto>
    {
        private readonly IAgentService _agentService;

        public CreateAgentEndpoint(IAgentService agentService)
        {
            _agentService = agentService;
        }

        public override void Configure()
        {
            Post("/agents/create");
            AllowAnonymous();
            Options(x =>
            {
                x.WithSummary("Creates a new agent.")
                 .WithDescription("Creates a new agent using the provided data and returns the created agent.");
            });
        }

        public override async Task HandleAsync(CreateAgentRequest req, CancellationToken ct)
        {
            try
            {
                var result = await _agentService.CreateAgentAsync(req);
                await SendAsync(result, cancellation: ct);
            }
            catch (ValidationException vex)
            { 
                AddError(vex.Message);
                ThrowIfAnyErrors();
            }
            catch (FriendlyBusinessException cex)
            {
                AddError(cex.Message);
                ThrowIfAnyErrors();
            }
        }
    }
}
