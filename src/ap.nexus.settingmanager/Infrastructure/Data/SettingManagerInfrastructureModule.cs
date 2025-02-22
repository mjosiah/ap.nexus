using ap.nexus.abstractions.Frameworks.SettingManagement;
using ap.nexus.settingmanager.Infrastructure.Data;
using AP.Nexus.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ap.nexus.settingmanager.Application
{
    public class SettingManagerInfrastructureModule : NexusModule
    {
       public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISettingStore, EntityFrameworkSettingStore>();
            //services.AddDbContext<SettingsDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        }
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
        }
    }
}
