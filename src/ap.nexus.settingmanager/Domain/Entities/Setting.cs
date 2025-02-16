using ap.nexus.core.domain;

namespace ap.nexus.settingmanager.Domain.Entities
{
    public class Setting : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public Guid? TenantId { get; set; }
        public string? UserId { get; set; } = string.Empty;
       
    }
}
