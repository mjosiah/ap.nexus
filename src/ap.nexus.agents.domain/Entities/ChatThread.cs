﻿using ap.nexus.core.domain;

namespace ap.nexus.agents.domain.Entities
{
    public class ChatThread : AuditableEntity
    {

        public string Title { get; set; } = string.Empty;
        public Guid AgentId { get; set; }

        public string UserId { get; set; } = string.Empty; 

        // Navigation properties
        public virtual Agent Agent { get; set; } 
    }
}
