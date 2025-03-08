using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ap.nexus.agents.api.contracts
{
    public class GetUserThreadsRequest : PagedAndSortedResultRequestDto
    {
        public string? UserId { get; set; }
    }
}
