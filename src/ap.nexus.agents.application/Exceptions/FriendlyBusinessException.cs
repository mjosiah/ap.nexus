using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ap.nexus.agents.application.Exceptions
{
    
    public class FriendlyBusinessException : Exception
    {
        public FriendlyBusinessException(string message) : base(message)
        {
        }
    }
}
