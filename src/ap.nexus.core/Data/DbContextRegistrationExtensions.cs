using ap.nexus.core.data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ap.nexus.core.Data
{
    public static class DbContextRegistrationExtensions
    {
        public static IServiceCollection AddNexusDbContext<TDbContext>(
            this IServiceCollection services,
            Action<DbContextOptions<TDbContext>> optionsAction)
            where TDbContext : DbContext
        {
            var options = new DbContextOptions<TDbContext>();
            optionsAction(options);

            // Register DbContext
            if (options.DbContextOptionsAction != null)
            {
                services.AddDbContext<TDbContext>(options.DbContextOptionsAction);
            }

            if (options.RegisterDefaultRepositories && options.DefaultRepositoryType != null)
            {
                RegisterDefaultRepositories(services, options);
            }

            RegisterCustomRepositories(services, options);

            return services;
        }

        private static void RegisterDefaultRepositories<TDbContext>(
            IServiceCollection services,
            DbContextOptions<TDbContext> options) where TDbContext : DbContext
        {
            var entityTypes = typeof(TDbContext)
                .GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                           p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(p => p.PropertyType.GetGenericArguments()[0]);

            foreach (var entityType in entityTypes)
            {
                var genericRepositoryType = options.DefaultRepositoryType!.MakeGenericType(entityType);
                var genericInterface = typeof(IGenericRepository<>).MakeGenericType(entityType);
                services.AddScoped(genericInterface, genericRepositoryType);
            }
        }

        private static void RegisterCustomRepositories<TDbContext>(
            IServiceCollection services,
            DbContextOptions<TDbContext> options) where TDbContext : DbContext
        {
            foreach (var (entityType, repositoryType) in options.CustomRepositories)
            {
                services.AddScoped(
                    typeof(IGenericRepository<>).MakeGenericType(entityType),
                    repositoryType);
            }
        }
    }
}
