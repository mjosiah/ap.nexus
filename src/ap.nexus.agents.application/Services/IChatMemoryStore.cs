using Microsoft.SemanticKernel.ChatCompletion;

namespace ap.nexus.agents.application.Services
{
    public interface IChatMemoryStore
    {
        Task<ChatHistory?> GetChatHistoryAsync(Guid externalId);
        Task SetChatHistoryAsync(Guid externalId, ChatHistory chatHistory);
        Task RemoveChatHistoryAsync(Guid externalId);
        Task<bool> ExistsAsync(Guid externalId);
        int GetThreadCount();
        IEnumerable<string> GetInactiveThreads(TimeSpan inactivityThreshold, DateTime currentTime);
        Task PruneInactiveThreadsAsync(TimeSpan inactivityThreshold, DateTime currentTime); 
    }
}
