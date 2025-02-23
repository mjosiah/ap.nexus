using ap.nexus.agents.infrastructure.Data;
using ap.nexus.agents.infrastructure.Data.Repositories;
using ap.nexus.agents.infrastructure.DateTimeProviders;
using ap.nexus.core.Data;
using AP.Nexus.Core.Modularity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ap.nexus.agents.infrastructure
{
    public class AgentsInfrastructureModule : NexusModule
    {
        public override void ConfigureModuleServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNexusDbContext<AgentsDbContext>(options =>
            {
                options.DbContextOptionsAction = builder =>
                    builder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

                options.AddDefaultRepositories(typeof(GenericRepository<>));


            });

            services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        }

        public override async Task InitializeAsync()
        {
            // Get the DbContext from service provider
            using var scope = ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AgentsDbContext>();

            // Apply any pending migrations
            await dbContext.Database.MigrateAsync();
            DatabaseSeeder.Seed(dbContext);

            await base.InitializeAsync();
        }
    }
}
