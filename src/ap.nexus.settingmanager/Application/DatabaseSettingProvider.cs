using ap.nexus.abstractions.Frameworks.SettingManagement;
using ap.nexus.settingmanager.Domain.Entities;
using ap.nexus.settingmanager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ap.nexus.settingmanager.Application
{
    public class DatabaseSettingProvider : ISettingProvider
    {
        private readonly SettingsDbContext _dbContext;

        public string Name => "Database";

        public DatabaseSettingProvider(SettingsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GetOrNullAsync(string name, Guid? tenantId = null, string? userId = null)
        {
            var setting = await _dbContext.Settings
                .FirstOrDefaultAsync(s =>
                    s.Name == name &&
                    s.TenantId == tenantId &&
                    s.UserId == userId);

            return setting?.Value;
        }

        public async Task SetAsync(string name, string value, Guid? tenantId = null, string? userId = null)
        {
            var setting = await _dbContext.Settings
                .FirstOrDefaultAsync(s =>
                    s.Name == name &&
                    s.TenantId == tenantId &&
                    s.UserId == userId);

            if (setting == null)
            {
                setting = new Setting
                {
                    Name = name,
                    TenantId = tenantId,
                    UserId = userId,
                    ProviderName = Name,
                };
                _dbContext.Settings.Add(setting);
            }

            setting.Value = value;

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(string name, Guid? tenantId = null, string? userId = null)
        {
            var setting = await _dbContext.Settings
                .FirstOrDefaultAsync(s =>
                    s.Name == name &&
                    s.TenantId == tenantId &&
                    s.UserId == userId);

            if (setting != null)
            {
                _dbContext.Settings.Remove(setting);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
