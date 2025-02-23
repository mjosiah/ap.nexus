using ap.nexus.core.domain;

namespace ap.nexus.settingmanager.Domain.Entities
{
    public class Setting : AuditableEntity
    {
        /// <summary>
        /// Name of the setting (e.g., "SmartDocs.BatchSize")
        /// </summary>
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Serialized value of the setting
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Provider that manages this setting
        /// </summary>
        public string ProviderName { get; set; } = string.Empty;

        /// <summary>
        /// Optional tenant ID for multi-tenant settings
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Optional user ID for user-specific settings
        /// </summary>
        public string? UserId { get; set; }
    }
}
