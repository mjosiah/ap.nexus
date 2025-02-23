using ap.nexus.abstractions.Frameworks.SettingManagement;
using ap.nexus.core.SettingManagement.Providers;
using Microsoft.Extensions.Configuration;

namespace ap.nexus.settingmanager.Application.Providers
{
    public class ConfigurationSettingValueProvider : SettingValueProviderBase
    {
        private readonly IConfiguration _configuration;

        public ConfigurationSettingValueProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override string Name => "Configuration";

        public override Task<string> GetOrNullAsync(ISettingDefinition setting, Guid? tenantId = null, string? userId = null)
        {
            return Task.FromResult(_configuration[setting.Name]);
        }
    }
}
