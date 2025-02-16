using ap.nexus.core.Settings.Definitions;
using ap.nexus.core.Settings;
using System.Text.Json;

namespace ap.nexus.settingmanager.Application
{
    public class SettingManager : ISettingManager
    {
        private readonly IDictionary<string, ISettingDefinition> _settingDefinitions;
        private readonly IDictionary<string, ISettingProvider> _settingProviders;

        public SettingManager(
            IEnumerable<ISettingProvider> providers)
        {
            _settingDefinitions = new Dictionary<string, ISettingDefinition>();
            _settingProviders = new Dictionary<string, ISettingProvider>();

            foreach (var provider in providers)
            {
                _settingProviders[provider.Name] = provider;
            }
        }

        public async Task DefineSettingsAsync(IEnumerable<ISettingDefinition> definitions)
        {
            foreach (var definition in definitions)
            {
                _settingDefinitions[definition.Name] = definition;
            }
        }

        public async Task<T> GetSettingValueAsync<T>(string name, Guid? tenantId = null, string? userId = null)
        {
            if (!_settingDefinitions.TryGetValue(name, out var definition))
            {
                throw new ArgumentException($"Setting '{name}' is not defined");
            }

            var provider = _settingProviders.TryGetValue(definition.ProviderName, out var settingProvider)
    ? settingProvider
    : null;

            var value = await provider.GetOrNullAsync(name, tenantId, userId);

            if (value == null)
            {
                return (T)definition.DefaultValue;
            }

            return typeof(T) == typeof(string)
                ? (T)(object)value
                : JsonSerializer.Deserialize<T>(value);
        }

        public async Task SetSettingValueAsync<T>(string name, T value, Guid? tenantId = null, string? userId = null)
        {
            if (!_settingDefinitions.TryGetValue(name, out var definition))
            {
                throw new ArgumentException($"Setting '{name}' is not defined");
            }

            ValidateSettingValue(definition, value);

            var provider = _settingProviders.TryGetValue(definition.ProviderName, out var settingProvider)
    ? settingProvider
    : null;

            var serializedValue = typeof(T) == typeof(string)
                ? (string)(object)value
                : JsonSerializer.Serialize(value);

            await provider.SetAsync(name, serializedValue, tenantId, userId);
        }

        private void ValidateSettingValue<T>(ISettingDefinition definition, T value)
        {
            // Implementation of validation logic based on setting definition type
            switch (definition)
            {
                case StringSettingDefinition stringDef:
                    ValidateStringValue(stringDef, value as string);
                    break;
                case NumberSettingDefinition numberDef:
                    ValidateNumberValue(numberDef, Convert.ToDouble(value));
                    break;
                    // Add other validation cases
            }
        }

        private void ValidateStringValue(StringSettingDefinition definition, string value)
        {
            if (value == null) return;

            if (definition.MinLength.HasValue && value.Length < definition.MinLength.Value)
            {
                throw new ArgumentException($"Value length must be at least {definition.MinLength.Value}");
            }

            if (definition.MaxLength.HasValue && value.Length > definition.MaxLength.Value)
            {
                throw new ArgumentException($"Value length must not exceed {definition.MaxLength.Value}");
            }

            if (definition.AllowedValues?.Count > 0 && !definition.AllowedValues.Contains(value))
            {
                throw new ArgumentException($"Value must be one of: {string.Join(", ", definition.AllowedValues)}");
            }
        }

        private void ValidateNumberValue(NumberSettingDefinition definition, double value)
        {
            if (definition.Minimum.HasValue && value < definition.Minimum.Value)
            {
                throw new ArgumentException($"Value must be at least {definition.Minimum.Value}");
            }

            if (definition.Maximum.HasValue && value > definition.Maximum.Value)
            {
                throw new ArgumentException($"Value must not exceed {definition.Maximum.Value}");
            }
        }
    }
}
