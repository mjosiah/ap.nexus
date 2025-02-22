using ap.nexus.abstractions.Frameworks.SettingManagement;
using ap.nexus.settingmanager.Infrastructure.Data;
using AP.Nexus.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using ap.nexus.core.data;
using ap.nexus.settingmanager.Infrastructure.Data.Repositories;

namespace ap.nexus.settingmanager.Application
{
    public class SettingManagerInfrastructureModule : NexusModule
    {
       public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISettingStore, EntityFrameworkSettingStore>();
            services.AddDbContext<SettingsDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Register the generic repository.
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericSettingRepository<>));
        }
        public override async Task InitializeAsync()
        {
            // Get the DbContext from service provider
            using var scope = ServiceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SettingsDbContext>();

            // Apply any pending migrations
            await dbContext.Database.MigrateAsync();

            await base.InitializeAsync();
        }
    }
}
