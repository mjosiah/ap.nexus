namespace ap.nexus.core.Settings.Definitions
{
    public class NumberSettingDefinition : SettingDefinition
    {
        public double? Minimum { get; set; }
        public double? Maximum { get; set; }
        public double? Step { get; set; }

        public NumberSettingDefinition(string name) : base() { }
    }
}
