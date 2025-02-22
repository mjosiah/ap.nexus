namespace ap.nexus.abstractions.Frameworks.SettingManagement
{
    public interface ISettingDefinition
    {
        string Name { get; }
        string DisplayName { get; }
        string Description { get; }
        object DefaultValue { get; }
        bool IsEncrypted { get; }
        bool IsNullable { get; }
        string ProviderName { get; }
    }
}
