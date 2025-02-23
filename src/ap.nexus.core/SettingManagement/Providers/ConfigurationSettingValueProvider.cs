using ap.nexus.abstractions.Frameworks.SettingManagement;
using Microsoft.Extensions.Configuration;

namespace ap.nexus.core.SettingManagement.Providers
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
