using ap.nexus.agents.domain.Common;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ap.nexus.agents.domain.Entities
{
    public class ChatMessage : AuditableEntity
    {
        public Guid ExternalId { get; set; } // Globally unique identifier

        public string Content { get; set; } = string.Empty; //A convenience property to get or set the text of the first item.
        
        public string ItemsJson { get; set; } = string.Empty; // Serialised ChatMessageContentItemCollection from semantic kernel.
        public int ChatThreadId { get; set; }

        public string UserId { get; set; } = string.Empty; // Links to the User

        public Role Role { get; set; }
        public string MetadataJson { get; set; } = string.Empty; // Metadata for the message

        public string Source { get; set; } = string.Empty; // Source of the message

        // Navigation properties
        public virtual ChatThread ChatThread { get; set; }

        public ChatMessageContentItemCollection GetChatMessageContentItems() => System.Text.Json.JsonSerializer.Deserialize<ChatMessageContentItemCollection>(ItemsJson) ?? new();
    }

    public enum Role
    {
        User,
        Assistant,
        System
    }
}
