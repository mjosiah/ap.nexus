using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ap.nexus.abstractions.Agents.Enums
{

    public enum ScopeType
    {
        Personal,
        Team,
        Enterprise
    }

    public enum ReasoningEffort
    {
        Low,
        Medium,
        High
    }

    public enum ToolType
    {
        CodeInterpreter,
        FileSearch,
        Function
    }
}
