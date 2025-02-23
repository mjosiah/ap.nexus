using ap.nexus.core.data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ap.nexus.core.Data
{
    public class DbContextOptions<TDbContext> where TDbContext : DbContext
    {
        public bool RegisterDefaultRepositories { get; set; }
        public Type? DefaultRepositoryType { get; set; }
        public Dictionary<Type, Type> CustomRepositories { get; } = new();
        public Action<DbContextOptionsBuilder>? DbContextOptionsAction { get; set; }

        public DbContextOptions<TDbContext> AddDefaultRepositories(Type repositoryType)
        {
            RegisterDefaultRepositories = true;
            DefaultRepositoryType = repositoryType;
            return this;
        }

        public DbContextOptions<TDbContext> AddRepository<TEntity, TRepository>()
            where TEntity : class
            where TRepository : class, IGenericRepository<TEntity>
        {
            CustomRepositories[typeof(TEntity)] = typeof(TRepository);
            return this;
        }
    }
}
