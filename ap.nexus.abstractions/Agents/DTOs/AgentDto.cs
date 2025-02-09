﻿using ap.nexus.abstractions.Agents.Enums;

namespace ap.nexus.abstractions.Agents.DTOs
{
    public class AgentDto
    {
        public Guid Id { get; set; }
        public Guid ExternalId { get; set; } // Unique external identifier for distributed systems
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Instruction { get; set; } = string.Empty; // Single string for instructions
        public ReasoningEffort? ReasoningEffort { get; set; }
        public List<ToolConfigurationDto> Tools { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new();
        public ScopeType Scope { get; set; }
        public string ScopeExternalId { get; set; } = string.Empty; // Holds Team, User, or Enterprise identifier
    }
}
