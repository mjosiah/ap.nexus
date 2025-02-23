using ap.nexus.agents.infrastructure.Data;
using ap.nexus.agents.infrastructure.DateTimeProviders;
using AP.Nexus.Core.Modularity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ap.nexus.agents.infrastructure
{
    public class AgentsInfrastructureModule : NexusModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register the AgentsDbContext using SQL Server or In-Memory for tests.
            services.AddDbContext<AgentsDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Register the generic repository.
            services.Scan(scan => scan
            .FromAssemblyOf<AgentsDbContext>()
            .AddClasses(classes => classes.Where(type => type.Namespace.Contains("ap.nexus.agents.infrastructure")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

            services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        }
    }
}
