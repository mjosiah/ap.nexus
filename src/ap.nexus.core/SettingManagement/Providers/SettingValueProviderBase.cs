using ap.nexus.abstractions.Frameworks.SettingManagement;
using ap.nexus.core.Settings.Definitions;

namespace ap.nexus.core.SettingManagement.Providers
{
    public abstract class SettingValueProviderBase : ISettingValueProvider
    {
        public abstract string Name { get; }
        public abstract Task<string> GetOrNullAsync(ISettingDefinition setting, Guid? tenantId = null, string? userId = null);

       
    }
}
