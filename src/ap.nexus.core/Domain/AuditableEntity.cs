using System.ComponentModel.DataAnnotations;

namespace ap.nexus.core.domain
{
    public class AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }
}
