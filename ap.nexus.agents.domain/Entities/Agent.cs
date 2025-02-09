using ap.nexus.abstractions.Agents.Enums;
using ap.nexus.agents.domain.Common;

namespace ap.nexus.agents.domain.Entities
{
    public class Agent : AuditableEntity
    {
        public Guid ExternalId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Instruction { get; set; } = string.Empty;
        public ReasoningEffort? ReasoningEffort { get; set; }
        public string ScopeExternalId { get; set; } = string.Empty;
        public ScopeType Scope { get; set; }
        public string ToolsJson { get; set; } = string.Empty;
    }
}
