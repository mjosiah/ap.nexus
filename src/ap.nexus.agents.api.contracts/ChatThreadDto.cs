using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ap.nexus.agents.api.contracts
{
    public class ChatThreadDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid AgentId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
