﻿using ap.nexus.agents.domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ap.nexus.agents.infrastructure.Data
{
    public class AgentsDbContext : DbContext
    {
        public AgentsDbContext(DbContextOptions<AgentsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Agent> Agents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Agent>(entity =>
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
