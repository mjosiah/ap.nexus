using ap.nexus.agents.domain.Common;

namespace ap.nexus.agents.domain.Entities
{
    public class ChatThread : AuditableEntity
    {
        public Guid ExternalId { get; set; } // Globally unique identifier

        public string Title { get; set; } = string.Empty;
        public int AgentId { get; set; }

        public Guid AgentExternalId { get; set; } // Links to the Agent

        public string UserId { get; set; } = string.Empty; // Links to the User


        // Navigation properties
        public virtual Agent Agent { get; set; } 
    }
}
