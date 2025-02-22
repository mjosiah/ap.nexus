using ap.nexus.abstractions.Frameworks.SettingManagement;
using AP.Nexus.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ap.nexus.settingmanager.Application
{
    public class SettingManagerApplicationModule : NexusModule
    {
       public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISettingManager, SettingManager>();
        }
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
        }
    }
}
