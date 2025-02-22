namespace ap.nexus.abstractions.Frameworks.SettingManagement
{
    public interface ISettingValueProvider
    {
        string Name { get; }
        Task<string> GetOrNullAsync(ISettingDefinition setting, Guid? tenantId = null, string? userId = null);
    }
}
