namespace ap.nexus.abstractions.Frameworks.SettingManagement
{
    public interface ISettingManager
    {
        Task DefineSettingsAsync(IEnumerable<ISettingDefinition> definitions);
        Task<T> GetSettingValueAsync<T>(string name, Guid? tenantId = null, string? userId = null);

        Task SetSettingValueAsync<T>(string name, T value, Guid? tenantId = null, string? userId = null);
    }
}
