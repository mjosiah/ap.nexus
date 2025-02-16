using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ap.nexus.core.Settings.Definitions
{
    public interface ISettingDefinition
    {
        string Name { get; }
        string DisplayName { get; }
        string Description { get; }
        object DefaultValue { get; }
        bool IsEncrypted { get; }
        string ProviderName { get; }
    }
}
