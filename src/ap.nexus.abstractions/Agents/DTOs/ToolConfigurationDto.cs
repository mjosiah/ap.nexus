using ap.nexus.abstractions.Agents.Enums;

namespace ap.nexus.abstractions.Agents.DTOs
{
    public class ToolConfigurationDto
    {
        public ToolType Type { get; set; }
        public List<Guid>? FileIds { get; set; } = new();
        public List<Guid>? VectorStoreIds { get; set; } = new();
        public string? ToolId { get; set; }
    }
}
