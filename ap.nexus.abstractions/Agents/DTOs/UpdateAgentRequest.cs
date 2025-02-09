using ap.nexus.abstractions.Agents.Enums;

namespace ap.nexus.abstractions.Agents.DTOs
{
    public class UpdateAgentRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Model { get; set; }
        public string? Instructions { get; set; }
        public ReasoningEffort? ReasoningEffort { get; set; }
        public List<ToolConfigurationDto>? Tools { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
