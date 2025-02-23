using ap.nexus.abstractions.Frameworks.SettingManagement;
using ap.nexus.settingmanager.Infrastructure.Data;
using AP.Nexus.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ap.nexus.settingmanager.Infrastructure.Data.Repositories;
using ap.nexus.core.Data;

namespace ap.nexus.settingmanager.Application
{
    public class SettingManagerInfrastructureModule : NexusModule
    {
       public override void ConfigureModuleServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISettingStore, EntityFrameworkSettingStore>();

            services.AddNexusDbContext<SettingsDbContext>(options =>
            {
                options.DbContextOptionsAction = builder =>
                    builder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

                options.AddDefaultRepositories(typeof(GenericSettingRepository<>));

               
            });
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
