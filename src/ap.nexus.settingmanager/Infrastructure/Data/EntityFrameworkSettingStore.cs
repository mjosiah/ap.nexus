using ap.nexus.abstractions.Frameworks.SettingManagement;
using ap.nexus.core.data;
using ap.nexus.settingmanager.Domain.Entities;

namespace ap.nexus.settingmanager.Infrastructure.Data
{
    public class EntityFrameworkSettingStore : ISettingStore
    {
        private readonly IGenericRepository<Setting> _settingRepository;

        public EntityFrameworkSettingStore(IGenericRepository<Setting> settingRepository)
        {
            _settingRepository = settingRepository;
        }

        public async Task<string> GetOrNullAsync(string name, Guid? tenantId = null, string? userId = null)
        {
            var setting = await _settingRepository.FirstOrDefaultAsync(s =>
                s.Name == name &&
                s.TenantId == tenantId &&
                s.UserId == userId);

            return setting?.Value;
        }

        public async Task SetAsync(string name, string value, Guid? tenantId = null, string? userId = null)
        {
            var setting = await _settingRepository.FirstOrDefaultAsync(s =>
                s.Name == name &&
                s.TenantId == tenantId &&
                s.UserId == userId);

            if (setting == null)
            {
                setting = new Setting
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Value = value,
                    TenantId = tenantId,
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "system" // Should come from current user context
                };

                await _settingRepository.AddAsync(setting);
            }
            else
            {
                setting.Value = value;
                setting.LastModifiedDate = DateTime.UtcNow;
                setting.LastModifiedBy = "system"; // Should come from current user context

                await _settingRepository.UpdateAsync(setting);
            }

            await _settingRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(string name, Guid? tenantId = null, string? userId = null)
        {
            var setting = await _settingRepository.FirstOrDefaultAsync(s =>
                s.Name == name &&
                s.TenantId == tenantId &&
                s.UserId == userId);

            if (setting != null)
            {
                await _settingRepository.DeleteAsync(setting);
                await _settingRepository.SaveChangesAsync();
            }
        }

        public async Task InitializeSettingsAsync(IEnumerable<ISettingDefinition> definitions, Guid? tenantId = null)
        {
            foreach (var definition in definitions)
            {
                // Check if setting exists
                var existingSetting = await _settingRepository.FirstOrDefaultAsync(s =>
                    s.Name == definition.Name &&
                    s.TenantId == tenantId &&
                    s.UserId == null);

                // Only create if it doesn't exist
                if (existingSetting == null && definition.DefaultValue != null)
                {
                    var setting = new Setting
                    {
                        Id = Guid.NewGuid(),
                        Name = definition.Name,
                        Value = definition.DefaultValue.ToString(),
                        TenantId = tenantId,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = "system"
                    };

                    await _settingRepository.AddAsync(setting);
                }
            }

            await _settingRepository.SaveChangesAsync();
        }
    }
}
