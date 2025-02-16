using ap.nexus.core.Settings.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ap.nexus.settingmanager.Application
{
    public interface ISettingManager
    {
        Task DefineSettingsAsync(IEnumerable<ISettingDefinition> definitions);
        Task<T> GetSettingValueAsync<T>(string name, Guid? tenantId = null, string? userId = null);

        Task SetSettingValueAsync<T>(string name, T value, Guid? tenantId = null, string? userId = null);
    }
}
