namespace ap.nexus.abstractions.Agents.DTOs
{
    public class ChatThread
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid AgentId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
