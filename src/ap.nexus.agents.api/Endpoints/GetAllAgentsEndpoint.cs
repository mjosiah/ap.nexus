using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.application.Exceptions;
using FastEndpoints;
using System.ComponentModel.DataAnnotations;


namespace ap.nexus.agents.api.Endpoints
{
    public class GetAllAgentsEndpoint : Endpoint<PagedAndSortedResultRequestDto, PagedResultDto<AgentDto>>
    {
        private readonly IAgentService _agentService;
        public GetAllAgentsEndpoint(IAgentService agentService)
        {
            _agentService = agentService;
        }

        public override void Configure()
        {
            Get("/agents");
            AllowAnonymous();
            Options(x =>
            {
                x.WithSummary("Retrieves a paged list of agents.")
                 .WithDescription("Returns a paged list of agents based on the provided paging and sorting parameters.");
            });
        }

        public override async Task HandleAsync(PagedAndSortedResultRequestDto req, CancellationToken ct)
        {
            try
            {
                var pagedResult = await _agentService.GetAllAgentsAsync(req);
                await SendAsync(pagedResult, cancellation: ct);
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
