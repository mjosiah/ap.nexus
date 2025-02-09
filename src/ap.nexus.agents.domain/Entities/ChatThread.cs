using ap.nexus.agents.domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ap.nexus.agents.domain.Entities
{
    public class ChatThread : AuditableEntity
    {
        

        public Guid ExternalId { get; set; } // Globally unique identifier

        public string Title { get; set; } = string.Empty;
        public int AgentId { get; set; }

        public Guid AgentExternalId { get; set; } // Links to the Agent

        public string UserId { get; set; } // Links to the User


        // Navigation properties (if you have related entities)
        public virtual Agent Agent { get; set; } // If you have an Agent entity

    }
}
