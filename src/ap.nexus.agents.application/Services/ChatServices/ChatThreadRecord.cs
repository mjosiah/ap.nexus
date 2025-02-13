using Microsoft.SemanticKernel.ChatCompletion;

namespace ap.nexus.agents.application.Services.ChatServices
{
    public class ChatThreadRecord
    {
        public ChatHistory ChatHistory { get; }
        public DateTime? LastAccessed { get; private set; }

        public ChatThreadRecord(ChatHistory chatHistory)
        {
            ChatHistory = chatHistory;
            LastAccessed = DateTime.UtcNow;
        }

        public void UpdateLastAccessed() => LastAccessed = DateTime.UtcNow;

    }
}
