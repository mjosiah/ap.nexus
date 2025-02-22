using ap.nexus.settingmanager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ap.nexus.settingmanager.Infrastructure.Data
{
    public class SettingsDbContext : DbContext
    {
        public DbSet<Setting> Settings { get; set; }

        public SettingsDbContext(DbContextOptions<SettingsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Setting>(builder =>
            {
                builder.HasKey(s => s.Id);

                builder.HasIndex(s => new { s.Name, s.TenantId, s.UserId })
                    .IsUnique();

                builder.Property(s => s.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                builder.Property(s => s.Value)
                    .IsRequired();

                builder.Property(s => s.ProviderName)
                    .IsRequired()
                    .HasMaxLength(50);
            });
        }
    }
}
