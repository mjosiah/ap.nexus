// AP.Nexus.Core/Extensions/ServiceCollectionExtensions.cs
using ap.nexus.core.Modularity;
using AP.Nexus.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AP.Nexus.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddModule<TModule>(this IServiceCollection services, IConfiguration configuration)
            where TModule : NexusModule
        {
            // Create instance of module
            var module = Activator.CreateInstance<TModule>();

            // Register the module itself
            services.AddSingleton(module);

            // Configure services for the module
            module.ConfigureServices(services,configuration);

            return services;
        }

        public static async Task InitializeModulesAsync(this IServiceProvider serviceProvider)
        {
            var modules = serviceProvider.GetServices<NexusModule>();

            foreach (var module in modules)
            {
                // Set the ServiceProvider before initialization
                module.InternalServiceProvider = serviceProvider;

                // Initialize
                await module.InitializeAsync();

                // Run post-initialization
                module.OnApplicationInitialization();
            }
        }
    }
}