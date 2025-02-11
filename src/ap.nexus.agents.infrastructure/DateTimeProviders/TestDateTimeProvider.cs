using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ap.nexus.agents.infrastructure.DateTimeProviders
{
    public class TestDateTimeProvider : IDateTimeProvider
    {
        public DateTime Now { get; set; } = DateTime.Now.AddMinutes(-60); // You can set this in your tests
    }
}
