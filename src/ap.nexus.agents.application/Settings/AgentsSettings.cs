using ap.nexus.abstractions.Frameworks.SettingManagement;

namespace ap.nexus.agents.application.Settings
{
    public class AgentsSettings
    {
        private ISettingManager _settingManager;
        public AgentsSettings(ISettingManager settingManager)
        {
            _settingManager = settingManager;
        }

        //Methods to access settings
        public async Task<int> GetBatchSizeAsync(Guid? tenantId = null)
        {
            return await _settingManager.GetSettingValueAsync<int>(
                AgentsSettingDefinitions.BatchSize,
                tenantId);
        }

        public async Task<bool> IsOcrEnabledAsync(Guid? tenantId = null)
        {
            return await _settingManager.GetSettingValueAsync<bool>(
                AgentsSettingDefinitions.OcrEnabled,
                tenantId);
        }

        public async Task<OcrConfig> GetOcrConfigAsync(Guid? tenantId = null)
        {
            return await _settingManager.GetSettingValueAsync<OcrConfig>(
                AgentsSettingDefinitions.OcrConfig,
                tenantId);
        }
    }
}
