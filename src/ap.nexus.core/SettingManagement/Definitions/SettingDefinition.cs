using ap.nexus.abstractions.Frameworks.SettingManagement;

namespace ap.nexus.core.Settings.Definitions
{
    public class SettingDefinition : ISettingDefinition
    {
        public string Name { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public object DefaultValue { get; set; }   
        public bool IsEncrypted { get; set; }
        public bool IsNullable { get; set; }
        public string ProviderName { get; set; } = string.Empty;

       public SettingDefinition(string name)
        {
            Name = name;
        }
    }
}
