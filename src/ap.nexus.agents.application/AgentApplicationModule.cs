using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.abstractions.Frameworks.SettingManagement;
using ap.nexus.agents.application.Services.ChatServices;
using ap.nexus.agents.application.Services;
using ap.nexus.agents.application.Settings;
using ap.nexus.settingmanager.Infrastructure.Data;
using AP.Nexus.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace ap.nexus.agents.application
{
    public class AgentsApplicationModule : NexusModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register application services.
            services.AddScoped<IAgentService, AgentService>();
            services.AddScoped<IThreadService, ThreadService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IChatHistoryManager, ChatHistoryManager>();
            services.AddSingleton<IChatMemoryStore, InMemoryChatMemoryStore>();
        }

        public override async Task InitializeAsync()
        {
            using var scope = ServiceProvider.CreateScope();
            var settingManager = scope.ServiceProvider.GetRequiredService<ISettingManager>();
            var settingStore = scope.ServiceProvider.GetRequiredService<ISettingStore>();

            var definitions = AgentsSettingDefinitions.GetDefinitions();
            await settingManager.DefineSettingsAsync(definitions);

            if (settingStore is EntityFrameworkSettingStore efStore)
            {
                await efStore.InitializeSettingsAsync(definitions);
            }

            await base.InitializeAsync();
        }

    }
}
