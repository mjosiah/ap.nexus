using ap.nexus.abstractions.Agents.Enums;
using ap.nexus.core.domain;

namespace ap.nexus.agents.domain.Entities
{
    public class Agent : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Instruction { get; set; } = string.Empty;
        public ReasoningEffort? ReasoningEffort { get; set; }
        public string ScopeId { get; set; } = string.Empty;
        public ScopeType Scope { get; set; }
        public string Tools { get; set; } = string.Empty;
    }
}
