namespace ap.nexus.core.Settings.Definitions
{
    public class StringSettingDefinition : SettingDefinition
    {
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public IReadOnlyList<string> AllowedValues { get; set; }

        public StringSettingDefinition(string name) : base(name) { }
    }

}
