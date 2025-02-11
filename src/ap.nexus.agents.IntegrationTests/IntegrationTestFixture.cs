using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.application.Services;
using ap.nexus.agents.infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using ap.nexus.agents.infrastructure.Data.Repositories;
using Microsoft.Extensions.Logging;
using ap.nexus.agents.infrastructure.DateTimeProviders;

namespace ap.nexus.agents.IntegrationTests
{
    public class IntegrationTestFixture : IDisposable

    {
        private readonly SqliteConnection _connection;
        public ServiceProvider ServiceProvider { get; private set; }
        public AgentsDbContext DbContext { get; private set; }

        public IntegrationTestFixture()
        {
            // Create a new SQLite in‑memory connection.
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Set up a service collection.
            var services = new ServiceCollection();

            services.AddLogging(builder => builder.AddConsole());

            // Register the DbContext to use the SQLite in‑memory database.
            services.AddDbContext<AgentsDbContext>(options =>
                options.UseSqlite(_connection));

            // Create a dummy configuration (extend as needed).
            var configuration = new ConfigurationBuilder().Build();

            // Manually register the generic repository.
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Register application services.
            services.AddScoped<IAgentService, AgentService>();
            services.AddScoped<IThreadService, ThreadService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IChatHistoryManager, ChatHistoryManager>();
            services.AddTransient<IDateTimeProvider, TestDateTimeProvider>();

            ServiceProvider = services.BuildServiceProvider();

            // Retrieve the DbContext instance.
            DbContext = ServiceProvider.GetRequiredService<AgentsDbContext>();

            // Ensure the SQLite database is created.
            DbContext.Database.EnsureCreated();

            // Seed the database with initial test data.
            DatabaseSeeder.Seed(DbContext);
        }

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
            _connection.Close();
            _connection.Dispose();
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
