namespace ap.nexus.abstractions.Agents.DTOs
{
    public class CreateChatThreadRequest
    {
        public string Title { get; set; } = string.Empty;
        public Guid AgentId { get; set; } 
        public string UserId { get; set; } = string.Empty;
    }
}
