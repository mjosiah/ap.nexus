namespace ap.nexus.core.Settings
{
    public interface ISettingProvider
    {
        string Name { get; }
        Task<string> GetOrNullAsync(string name, Guid? tenantId = null, string? userId = null);
        Task SetAsync(string name, string value, Guid? tenantId = null, string? userId = null);
        Task DeleteAsync(string name, Guid? tenantId = null, string? userId = null);
    }
}
