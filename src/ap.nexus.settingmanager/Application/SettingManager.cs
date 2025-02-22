using ap.nexus.core.Settings.Definitions;
using System.Text.Json;
using ap.nexus.abstractions.Frameworks.SettingManagement;
using Microsoft.Extensions.Logging;
using ap.nexus.core.SettingManagement.Exceptions;

namespace ap.nexus.settingmanager.Application
{
    // AP.Nexus.SettingManagement.Application/Services/SettingManager.cs
    public class SettingManager : ISettingManager
    {
        private readonly IDictionary<string, ISettingDefinition> _settingDefinitions;
        private readonly IEnumerable<ISettingValueProvider> _providers;
        private readonly ISettingStore _settingStore;
        private readonly ILogger<SettingManager> _logger;

        public SettingManager(
            IEnumerable<ISettingValueProvider> providers,
            ISettingStore settingStore,
            ILogger<SettingManager> logger)
        {
            _settingDefinitions = new Dictionary<string, ISettingDefinition>();
            _providers = providers.OrderByDescending(p => GetProviderPriority(p.Name));
            _settingStore = settingStore;
            _logger = logger;
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
                throw new SettingNotFoundException(name);
            }

            // Validate the requested type matches the setting definition
            ValidateSettingType<T>(definition);

            // Try each provider in order
            foreach (var provider in _providers)
            {
                var value = await provider.GetOrNullAsync(definition, tenantId, userId);
                if (value != null)
                {
                    try
                    {
                        return ConvertValue<T>(definition, value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to convert setting {SettingName} value from provider {ProviderName}", name, provider.Name);
                    }
                }
            }

            // If no provider returns a value, return the default
            return definition.DefaultValue != null
                ? ConvertValue<T>(definition, definition.DefaultValue.ToString())
                : default;
        }

        public async Task SetSettingValueAsync<T>(string name, T value, Guid? tenantId = null, string? userId = null)
        {
            if (!_settingDefinitions.TryGetValue(name, out var definition))
            {
                throw new SettingNotFoundException(name);
            }

            ValidateSettingValue(definition, value);
            string stringValue = ConvertToString(value);
            await _settingStore.SetAsync(name, stringValue, tenantId, userId);
        }

        private void ValidateSettingType<T>(ISettingDefinition definition)
        {
            var requestedType = typeof(T);

            switch (definition)
            {
                case StringSettingDefinition _:
                    if (requestedType != typeof(string))
                        throw new InvalidSettingTypeException(definition.Name, typeof(string), requestedType);
                    break;

                case NumberSettingDefinition _:
                    if (!IsNumericType(requestedType))
                        throw new InvalidSettingTypeException(definition.Name, typeof(decimal), requestedType);
                    break;

                case BooleanSettingDefinition _:
                    if (requestedType != typeof(bool))
                        throw new InvalidSettingTypeException(definition.Name, typeof(bool), requestedType);
                    break;

                case JsonSettingDefinition jsonDef:
                    if (!requestedType.IsAssignableTo(jsonDef.SchemaType))
                        throw new InvalidSettingTypeException(definition.Name, jsonDef.SchemaType, requestedType);
                    break;
            }
        }

        private void ValidateSettingValue<T>(ISettingDefinition definition, T value)
        {
            switch (definition)
            {
                case StringSettingDefinition stringDef:
                    ValidateStringValue(stringDef, value as string);
                    break;

                case NumberSettingDefinition numberDef:
                    ValidateNumberValue(numberDef, Convert.ToDouble(value));
                    break;

                case JsonSettingDefinition jsonDef:
                    ValidateJsonValue(jsonDef, value);
                    break;
            }
        }

        private void ValidateStringValue(StringSettingDefinition definition, string value)
        {
            if (value == null && !definition.IsNullable)
                throw new SettingValidationException(definition.Name, "Value cannot be null","null");

            if (value != null)
            {
                if (definition.MinLength.HasValue && value.Length < definition.MinLength.Value)
                    throw new SettingValidationException(definition.Name, $"Value length must be at least {definition.MinLength.Value}", value);

                if (definition.MaxLength.HasValue && value.Length > definition.MaxLength.Value)
                    throw new SettingValidationException(definition.Name, $"Value length must not exceed {definition.MaxLength.Value}", value);

                if (definition.AllowedValues?.Count > 0 && !definition.AllowedValues.Contains(value))
                    throw new SettingValidationException(definition.Name, $"Value must be one of: {string.Join(", ", definition.AllowedValues)}", value);
            }
        }

        private void ValidateNumberValue(NumberSettingDefinition definition, double value)
        {
            if (definition.Minimum.HasValue && value < definition.Minimum.Value)
                throw new SettingValidationException(definition.Name, $"Value must be at least {definition.Minimum.Value}", value);

            if (definition.Maximum.HasValue && value > definition.Maximum.Value)
                throw new SettingValidationException(definition.Name, $"Value must not exceed {definition.Maximum.Value}", value);
        }

        private void ValidateJsonValue(JsonSettingDefinition definition, object value)
        {
            if (value == null && !definition.IsNullable)
                throw new SettingValidationException(definition.Name, "Value cannot be null", value);

            // Additional JSON schema validation could be implemented here
        }

        private T ConvertValue<T>(ISettingDefinition definition, string value)
        {
            try
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)value;

                if (definition is JsonSettingDefinition)
                    return JsonSerializer.Deserialize<T>(value);

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new SettingValueConversionException(definition.Name, typeof(T), ex);
            }
        }

        private string ConvertToString<T>(T value)
        {
            if (value == null)
                return null;

            if (value is string stringValue)
                return stringValue;

            if (value.GetType().IsClass)
                return JsonSerializer.Serialize(value);

            return value.ToString();
        }

        private bool IsNumericType(Type type)
        {
            return type == typeof(int) ||
                   type == typeof(long) ||
                   type == typeof(float) ||
                   type == typeof(double) ||
                   type == typeof(decimal);
        }

        private int GetProviderPriority(string providerName)
        {
            return providerName switch
            {
                "DefaultValue" => 1,
                "Configuration" => 2,
                "Global" => 3,
                _ => 99
            };
        }
    }
}
