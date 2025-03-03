using ap.nexus.agents.api.contracts;
using Refit;

namespace ap.nexus.agents.apiclient
{
    public interface IChatApiClient
    {
        [Post("/chat")]
        Task<ChatResponse> SendChatMessageAsync([Body] ChatRequest request);
    }
}
