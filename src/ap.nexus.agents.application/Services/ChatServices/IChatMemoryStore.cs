using Microsoft.SemanticKernel.ChatCompletion;

namespace ap.nexus.agents.application.Services.ChatServices
{
    public interface IChatMemoryStore
    {
        Task<ChatHistory?> GetChatHistoryAsync(Guid Id);
        Task SetChatHistoryAsync(Guid Id, ChatHistory chatHistory);
        Task RemoveChatHistoryAsync(Guid Id);
        Task<bool> ExistsAsync(Guid Id);
        int GetThreadCount();
        IEnumerable<string> GetInactiveThreads(TimeSpan inactivityThreshold, DateTime currentTime);
        Task PruneInactiveThreadsAsync(TimeSpan inactivityThreshold, DateTime currentTime);
    }
}
