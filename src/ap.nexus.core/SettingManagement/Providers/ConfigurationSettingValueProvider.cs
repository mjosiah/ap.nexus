using ap.nexus.abstractions.Frameworks.SettingManagement;
using ap.nexus.core.Settings.Definitions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
