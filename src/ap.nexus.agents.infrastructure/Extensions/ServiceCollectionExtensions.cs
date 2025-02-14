using ap.nexus.agents.infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ap.nexus.agents.infrastructure.Data.Repositories;
using ap.nexus.agents.infrastructure.DateTimeProviders;
using Microsoft.Data.Sqlite;

namespace ap.nexus.agents.infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the AgentsDbContext using SQL Server or In-Memory for tests.
            services.AddDbContext<AgentsDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Register the generic repository.
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddTransient<IDateTimeProvider, DateTimeProvider>();


            return services;
        }
    }
}
