using ap.nexus.abstractions.Frameworks.SettingManagement;

namespace ap.nexus.core.SettingManagement.Providers
{
    public class DefaultValueSettingValueProvider : SettingValueProviderBase
    {
        public override string Name => "DefaultValue";

        public override Task<string> GetOrNullAsync(ISettingDefinition setting, Guid? tenantId = null, string? userId = null)
        {
            return Task.FromResult(setting.DefaultValue?.ToString());
        }
    }
}
