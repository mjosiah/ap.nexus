using Microsoft.Extensions.DependencyInjection;

namespace ap.nexus.core.Modularity
{
    public interface IModuleManager
    {
        Task InitializeModulesAsync();
        void ConfigureServices(IServiceCollection services);
    }
}
