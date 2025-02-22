// AP.Nexus.Core/Modularity/NexusModule.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration) { }

        public virtual Task InitializeAsync() => Task.CompletedTask;

        public virtual void OnApplicationInitialization() { }

        protected T GetRequiredService<T>() where T : notnull
        {
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}