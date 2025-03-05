using ap.nexus.agents.domain.Entities;
using Microsoft.EntityFrameworkCore;
using ChatThread = ap.nexus.agents.domain.Entities.ChatThread;

namespace ap.nexus.agents.infrastructure.Data
{
    public class AgentsDbContext : DbContext
    {
        public AgentsDbContext(DbContextOptions<AgentsDbContext> options)
            : base(options)
        {
        }

        public DbSet<AgentEntity> Agents { get; set; }
        public DbSet<ChatThread> ChatThreads { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AgentEntity>(entity =>
            {
                // Global filter to ignore soft-deleted records.
                entity.HasQueryFilter(a => !a.IsDeleted);

                // Configure enum properties to be stored as strings.
                entity.Property(a => a.ReasoningEffort)
                      .HasConversion<string>();
                entity.Property(a => a.Scope)
                      .HasConversion<string>();
            });
        }
    }
}
