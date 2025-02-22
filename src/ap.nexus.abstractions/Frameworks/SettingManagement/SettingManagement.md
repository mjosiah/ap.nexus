# Setting Management System

## Overview
The Setting Management System provides a flexible and extensible way to manage application settings across different scopes (global, tenant, user) with support for different storage providers and value resolution strategies.

## Core Concepts

### Setting Definition
A setting definition describes a configurable setting in the system. It includes metadata about the setting such as its name, default value, and validation rules.

```csharp
public interface ISettingDefinition
{
    /// <summary>
    /// Unique name of the setting
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Display name for UI
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Description of the setting's purpose
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Default value if no other value is provided
    /// </summary>
    object DefaultValue { get; }

    /// <summary>
    /// Indicates if the value should be encrypted when stored
    /// </summary>
    bool IsEncrypted { get; }

    /// <summary>
    /// Provider that handles this setting
    /// </summary>
    string ProviderName { get; }
}
```

### Setting Store
The setting store interface defines how setting values are persisted and retrieved.

```csharp
public interface ISettingStore
{
    /// <summary>
    /// Gets a setting value or null if not found
    /// </summary>
    Task<string> GetOrNullAsync(string name, Guid? tenantId = null, Guid? userId = null);

    /// <summary>
    /// Sets a setting value
    /// </summary>
    Task SetAsync(string name, string value, Guid? tenantId = null, Guid? userId = null);

    /// <summary>
    /// Deletes a setting value
    /// </summary>
    Task DeleteAsync(string name, Guid? tenantId = null, Guid? userId = null);
}
```

### Setting Value Provider
Setting value providers determine how setting values are resolved. The system supports multiple providers in a chain of responsibility pattern.

```csharp
public interface ISettingValueProvider
{
    /// <summary>
    /// Unique name of the provider
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Attempts to get a setting value from this provider
    /// </summary>
    Task<string> GetOrNullAsync(ISettingDefinition setting, Guid? tenantId = null, Guid? userId = null);
}
```

## Value Resolution

The system resolves setting values using a chain of providers in the following order:

1. **DefaultValueSettingValueProvider**
   - Returns the default value specified in the setting definition
   - Used as a fallback when no other value is found

2. **ConfigurationSettingValueProvider**
   - Gets values from the application's configuration (appsettings.json, environment variables, etc.)
   - Useful for environment-specific settings

3. **GlobalSettingValueProvider**
   - Retrieves system-wide setting values from the setting store
   - Applied to all tenants and users

4. **TenantSettingValueProvider**
   - Gets tenant-specific values
   - Overrides global settings for a specific tenant

5. **UserSettingValueProvider**
   - Gets user-specific values
   - Highest priority, overrides all other values

## Implementation Guide

### Creating a Custom Setting Value Provider

```csharp
public class CustomSettingValueProvider : ISettingValueProvider
{
    public string Name => "Custom";

    public async Task<string> GetOrNullAsync(
        ISettingDefinition setting,
        Guid? tenantId = null,
        Guid? userId = null)
    {
        // Implement custom logic to retrieve setting value
        // Return null if no value is found
    }
}
```

### Implementing a Setting Store

```csharp
public class CustomSettingStore : ISettingStore
{
    public async Task<string> GetOrNullAsync(
        string name,
        Guid? tenantId = null,
        Guid? userId = null)
    {
        // Implement retrieval logic
    }

    public async Task SetAsync(
        string name,
        string value,
        Guid? tenantId = null,
        Guid? userId = null)
    {
        // Implement storage logic
    }

    public async Task DeleteAsync(
        string name,
        Guid? tenantId = null,
        Guid? userId = null)
    {
        // Implement deletion logic
    }
}
```

## Best Practices

1. **Setting Names**
   - Use hierarchical naming: `"Module:Category:SettingName"`
   - Example: `"Email:Smtp:Host"`
   - Makes settings easier to organize and find

2. **Default Values**
   - Always provide sensible default values
   - Document the implications of the default value

3. **Validation**
   - Implement validation in the setting manager
   - Consider using a validation provider pattern for complex rules

4. **Encryption**
   - Mark sensitive settings with `IsEncrypted = true`
   - Implement encryption in the setting store
   - Use secure encryption methods

5. **Caching**
   - Consider caching frequently accessed settings
   - Implement cache invalidation when settings change

## Common Scenarios

### Module-Specific Settings
```csharp
public static class EmailSettingDefinitions
{
    public static class Smtp
    {
        public static ISettingDefinition Host = new SettingDefinition(
            name: "Email:Smtp:Host",
            displayName: "SMTP Host",
            description: "SMTP server hostname",
            defaultValue: "smtp.yourserver.com"
        );

        public static ISettingDefinition Port = new SettingDefinition(
            name: "Email:Smtp:Port",
            displayName: "SMTP Port",
            description: "SMTP server port",
            defaultValue: 587
        );
    }
}
```

### Using Settings in Services
```csharp
public class EmailService
{
    private readonly ISettingManager _settingManager;

    public EmailService(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var host = await _settingManager.GetSettingValueAsync<string>("Email:Smtp:Host");
        var port = await _settingManager.GetSettingValueAsync<int>("Email:Smtp:Port");
        
        // Use settings to send email
    }
}
```

## Security Considerations

1. **Sensitive Data**
   - Use `IsEncrypted` for sensitive settings
   - Implement proper key management
   - Log access to sensitive settings

2. **Authorization**
   - Implement setting-level authorization
   - Consider using permission providers
   - Audit setting changes

3. **Validation**
   - Validate setting values before storage
   - Prevent injection attacks
   - Sanitize display values

## Troubleshooting

1. **Value Not Found**
   - Check provider chain order
   - Verify setting definition exists
   - Check scoping (tenant/user IDs)

2. **Type Conversion Errors**
   - Verify value format matches expected type
   - Implement custom type conversion if needed
   - Add proper error handling

3. **Performance Issues**
   - Implement caching
   - Review database indices
   - Monitor setting access patterns