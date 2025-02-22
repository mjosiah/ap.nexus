using ap.nexus.abstractions.Frameworks.SettingManagement;

namespace ap.nexus.core.Settings.Definitions
{
    public class SettingDefinition : ISettingDefinition
    {
        public string Name { get; private set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public object DefaultValue { get; set; }
        public bool IsEncrypted { get; set; }
        public bool IsNullable { get; set; }
        public string ProviderName { get; set; }

        protected SettingDefinition(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DisplayName = name;
            ProviderName = "Default";
        }
    }
}
