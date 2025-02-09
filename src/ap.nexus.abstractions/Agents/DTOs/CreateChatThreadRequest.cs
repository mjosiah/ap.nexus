using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ap.nexus.abstractions.Agents.DTOs
{
    public class CreateChatThreadRequest
    {
        public string Title { get; set; } = string.Empty;
        public Guid AgentExternalId { get; set; } 
        public string UserId { get; set; } = string.Empty;
    }
}
