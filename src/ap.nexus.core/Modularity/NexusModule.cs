using Microsoft.Extensions.DependencyInjection;

namespace ap.nexus.core.Modularity
{
    public abstract class NexusModule
    {
        public virtual void ConfigureServices(IServiceCollection services) { }
        public virtual Task InitializeAsync() => Task.CompletedTask;
        public virtual void OnApplicationInitialization() { }
    }
}
