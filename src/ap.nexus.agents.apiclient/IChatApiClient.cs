using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.agents.api.contracts;
using Refit;

namespace ap.nexus.agents.apiclient
{
    public interface IChatApiClient
    {
        [Post("/chat")]
        Task<ChatResponse> SendChatMessageAsync([Body] ChatRequest request);

        [Get("/agents")]
        Task<PagedResultDto<AgentDto>> GetAgentsAsync([Query] PagedAndSortedResultRequestDto request);
    }
}