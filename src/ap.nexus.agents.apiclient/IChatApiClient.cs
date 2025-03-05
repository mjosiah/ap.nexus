using ap.nexus.agents.api.contracts;
using Refit;

namespace ap.nexus.agents.apiclient
{
    public interface IChatApiClient
    {
        [Post("/chat")]
        Task<ChatResponse> SendChatMessageAsync([Body] ChatRequest request);

        [Get("/agents")]
        Task<api.contracts.PagedResultDto<AgentDto>> GetAgentsAsync([Query] api.contracts.PagedAndSortedResultRequestDto request);
    }
}