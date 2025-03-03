namespace ap.nexus.agents.website.Models
{
    public class ChatSessionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public Guid UserId { get; set; }
        public bool IsWebSearchEnabled { get; set; }
        public bool IsDeepThinkingEnabled { get; set; }
    }
}
