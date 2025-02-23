# Nexus Settings Management Module Documentation

## Overview

The Settings Management module provides a flexible and extensible system for managing application settings in Nexus modules. It supports different types of settings, multiple storage providers, and hierarchical configuration with support for tenant-specific and user-specific values.

## Core Concepts

### Setting Definitions

Settings in Nexus are defined using setting definitions that specify the name, type, and validation rules for each setting. The system supports several types of settings:

- **String Settings**: Text values with optional length constraints and allowed values
- **Number Settings**: Numeric values with optional minimum/maximum bounds
- **Boolean Settings**: True/false values
- **JSON Settings**: Complex objects that conform to a specified schema

### Setting Providers

The module uses a provider-based architecture with multiple providers that can supply setting values:

1. **Default Value Provider**: Provides the default values specified in setting definitions
2. **Configuration Provider**: Reads values from the application's configuration (appsettings.json)
3. **Global Provider**: Retrieves values from the persistent storage (database)

Providers are queried in order of priority, allowing for a flexible override system.

### Setting Store

The EntityFrameworkSettingStore provides persistent storage for setting values, supporting:
- Global settings
- Tenant-specific settings
- User-specific settings

## Implementation Guide

### 1. Define Your Settings

First, create a class to define your module's settings:

```csharp
public static class MyModuleSettingDefinitions
{
    // Constants for setting names
    public const string MaxItemCount = "MyModule:MaxItemCount";
    public const string DefaultLanguage = "MyModule:DefaultLanguage";
    public const string FeatureEnabled = "MyModule:FeatureEnabled";
    public const string ApiConfig = "MyModule:ApiConfig";

    public static IEnumerable<ISettingDefinition> GetDefinitions()
    {
        return new[]
        {
            // Number setting with validation
            new NumberSettingDefinition(MaxItemCount)
            {
                DisplayName = "Maximum Item Count",
                DefaultValue = 100,
                Minimum = 1,
                Maximum = 1000,
                Description = "Maximum number of items to process"
            },

            // String setting with allowed values
            new StringSettingDefinition(DefaultLanguage)
            {
                DisplayName = "Default Language",
                DefaultValue = "en",
                AllowedValues = new[] { "en", "es", "fr" },
                Description = "Default language for processing"
            },

            // Boolean setting
            new BooleanSettingDefinition(FeatureEnabled)
            {
                DisplayName = "Feature Enabled",
                DefaultValue = true,
                Description = "Enable or disable the feature"
            },

            // JSON setting for complex configuration
            new JsonSettingDefinition(ApiConfig, typeof(ApiConfiguration))
            {
                DisplayName = "API Configuration",
                DefaultValue = new ApiConfiguration
                {
                    Timeout = 30,
                    RetryCount = 3
                },
                Description = "API connection settings"
            }
        };
    }
}

public class ApiConfiguration
{
    public int Timeout { get; set; }
    public int RetryCount { get; set; }
}
```

### 2. Create a Settings Accessor

Create a settings class to provide strongly-typed access to your settings:

```csharp
public class MyModuleSettings : ITransientService
{
    private readonly ISettingManager _settingManager;

    public MyModuleSettings(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    public async Task<int> GetMaxItemCountAsync(Guid? tenantId = null)
    {
        return await _settingManager.GetSettingValueAsync<int>(
            MyModuleSettingDefinitions.MaxItemCount,
            tenantId);
    }

    public async Task<string> GetDefaultLanguageAsync(Guid? tenantId = null)
    {
        return await _settingManager.GetSettingValueAsync<string>(
            MyModuleSettingDefinitions.DefaultLanguage,
            tenantId);
    }

    public async Task<ApiConfiguration> GetApiConfigAsync(Guid? tenantId = null)
    {
        return await _settingManager.GetSettingValueAsync<ApiConfiguration>(
            MyModuleSettingDefinitions.ApiConfig,
            tenantId);
    }
}
```

### 3. Register Settings in Your Module

Initialize your settings during module startup:

```csharp
public class MyModuleApplicationModule : NexusModule
{
    private readonly ISettingManager _settingManager;

    public MyModuleApplicationModule(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        // Register your settings accessor
        services.AddScoped<MyModuleSettings>();
    }

    public override async Task InitializeAsync()
    {
        // Register module settings during initialization
        await _settingManager.DefineSettingsAsync(
            MyModuleSettingDefinitions.GetDefinitions());

        await base.InitializeAsync();
    }
}
```

### 4. Configure Settings in appsettings.json

Add your settings to the application configuration:

```json
{
  "MyModule": {
    "MaxItemCount": 500,
    "DefaultLanguage": "en",
    "FeatureEnabled": true,
    "ApiConfig": {
      "Timeout": 60,
      "RetryCount": 5
    }
  }
}
```

### 5. Use Settings in Your Code

Access settings in your services:

```csharp
public class MyService
{
    private readonly MyModuleSettings _settings;

    public MyService(MyModuleSettings settings)
    {
        _settings = settings;
    }

    public async Task ProcessItemsAsync(Guid? tenantId = null)
    {
        var maxItems = await _settings.GetMaxItemCountAsync(tenantId);
        var apiConfig = await _settings.GetApiConfigAsync(tenantId);

        // Use the settings...
    }
}
```

## Best Practices

1. **Setting Names**
   - Use consistent naming conventions
   - Prefix settings with module name
   - Use hierarchical structure (e.g., "MyModule:Category:SettingName")

2. **Validation**
   - Always set appropriate validation rules
   - Use MinLength/MaxLength for string settings
   - Set Minimum/Maximum for number settings
   - Define AllowedValues when applicable

3. **Default Values**
   - Always provide sensible default values
   - Document the reasoning behind default values
   - Consider different environments (development, production)

4. **Security**
   - Use IsEncrypted for sensitive settings
   - Be cautious with user-specific settings
   - Validate setting values before use

5. **Documentation**
   - Provide clear DisplayName and Description
   - Document any dependencies between settings
   - Include examples in setting descriptions

## Advanced Features

### Tenant-Specific Settings

The settings system supports multi-tenancy. Access tenant-specific settings by providing the tenantId:

```csharp
var tenantValue = await _settings.GetMaxItemCountAsync(tenantId);
```

### User-Specific Settings

For user-specific settings, provide both tenantId and userId:

```csharp
var userValue = await _settingManager.GetSettingValueAsync<string>(
    MyModuleSettingDefinitions.DefaultLanguage,
    tenantId,
    userId);
```

### Setting Value Resolution

Values are resolved in the following order:
1. User-specific value
2. Tenant-specific value
3. Global value
4. Configuration value
5. Default value

## Troubleshooting

### Common Issues

1. **Setting Not Found**
   - Verify setting name matches the definition
   - Check if settings are properly registered during initialization
   - Ensure setting definition is included in GetDefinitions()

2. **Invalid Setting Value**
   - Check validation rules in setting definition
   - Verify value type matches setting type
   - Review configuration values in appsettings.json

3. **Settings Not Persisting**
   - Confirm database connection
   - Check if migrations are applied
   - Verify proper service registration

### Debugging Tips

1. Enable detailed logging for the SettingManager:
```json
{
  "Logging": {
    "LogLevel": {
      "AP.Nexus.SettingManager": "Debug"
    }
  }
}
```

2. Use the SettingManager directly to inspect raw values:
```csharp
var rawValue = await _settingManager.GetSettingValueAsync<string>(
    settingName,
    tenantId,
    userId);
```

## Migration Guide

When updating existing settings:

1. Add new setting definitions first
2. Provide migration logic for existing values
3. Update configuration files
4. Deploy database migrations
5. Update code references

## Security Considerations

1. **Encrypted Settings**
   - Use IsEncrypted for sensitive data
   - Implement proper key management
   - Audit access to encrypted settings

2. **Access Control**
   - Implement authorization for setting management
   - Restrict user-specific settings
   - Log setting changes

3. **Validation**
   - Sanitize input values
   - Implement proper error handling
   - Validate setting values at runtime