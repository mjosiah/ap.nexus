
namespace ap.nexus.agents.api.contracts
{
    public class ToolConfigurationDto
    {
        public ToolType Type { get; set; }
        public List<Guid>? FileIds { get; set; } = new();
        public List<Guid>? VectorStoreIds { get; set; } = new();
        public string? ToolId { get; set; }
    }
}
