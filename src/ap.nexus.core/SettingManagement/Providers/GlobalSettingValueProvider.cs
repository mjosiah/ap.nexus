﻿using ap.nexus.abstractions.Frameworks.SettingManagement;

namespace ap.nexus.core.SettingManagement.Providers
{
    public class GlobalSettingValueProvider : SettingValueProviderBase
    {
        private readonly ISettingStore _settingStore;

        public GlobalSettingValueProvider(ISettingStore settingStore)
        {
            _settingStore = settingStore;
        }

        public override string Name => "Global";

        public override Task<string> GetOrNullAsync(ISettingDefinition setting, Guid? tenantId = null, string? userId = null)
        {
            return _settingStore.GetOrNullAsync(setting.Name);
        }
    }
}
