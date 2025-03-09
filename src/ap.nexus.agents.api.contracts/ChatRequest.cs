using Microsoft.SemanticKernel;
using System.ComponentModel.DataAnnotations;

namespace ap.nexus.agents.api.contracts
{
    public class ChatRequest
    {
        [Required]
        public Guid AgentId { get; set; }
        public Guid? ThreadId { get; set; }
        [Required]
        public ChatMessageContent Message { get; set; }
        public bool UseStreaming { get; set; } = false;
    }

    public class ChatResponse
    {
        public ChatMessageContent Response { get; set; } = new();
        public Guid ThreadId { get; set; }
    }
}
