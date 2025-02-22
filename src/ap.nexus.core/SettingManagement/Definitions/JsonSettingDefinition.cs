namespace ap.nexus.core.Settings.Definitions
{
    public class JsonSettingDefinition : SettingDefinition
    {
        public Type SchemaType { get; }

        public JsonSettingDefinition(string name, Type schemaType) : base()
        {
            SchemaType = schemaType ?? throw new ArgumentNullException(nameof(schemaType));
        }
    }
}
