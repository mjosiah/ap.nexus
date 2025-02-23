// AP.Nexus.Core/Modularity/NexusModule.cs
using ap.nexus.core.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AP.Nexus.Core.Modularity
{
    public abstract class NexusModule
    {
        private IServiceProvider _serviceProvider;

        protected IServiceProvider ServiceProvider
        {
            get => _serviceProvider ?? throw new InvalidOperationException(
                "ServiceProvider is not initialized. Make sure the module is properly registered.");
        }

        internal IServiceProvider InternalServiceProvider
        {
            get => _serviceProvider;
            set => _serviceProvider = value;
        }

        /// <summary>
        /// Gets the assemblies that should be scanned for service registration.
        /// By default, returns the assembly containing the derived module class.
        /// Override this property to include additional assemblies for scanning.
        /// </summary>
        protected virtual Assembly[] ServiceAssemblies => new[]
        {
            GetType().Assembly
        };

        /// <summary>
        /// Configures services for this module. This method is called during application startup.
        /// </summary>
        /// <param name="services">The service collection to configure</param>
        /// <param name="configuration">The application configuration</param>
        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register services from the module's assemblies
            RegisterModuleServices(services);

            // Allow derived modules to perform additional configuration
            ConfigureModuleServices(services, configuration);
        }

        /// <summary>
        /// Override this method to add module-specific service configuration.
        /// This is called after the automatic service registration.
        /// </summary>
        public virtual void ConfigureModuleServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        /// <summary>
        /// Registers services from the module's assemblies using the lifecycle interfaces.
        /// </summary>
        private void RegisterModuleServices(IServiceCollection services)
        {
            // We'll implement this in the ServiceCollectionExtensions class
            services.AddNexusServices(ServiceAssemblies);
        }

        public virtual Task InitializeAsync() => Task.CompletedTask;

        public virtual void OnApplicationInitialization() { }

        protected T GetRequiredService<T>() where T : notnull
        {
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}