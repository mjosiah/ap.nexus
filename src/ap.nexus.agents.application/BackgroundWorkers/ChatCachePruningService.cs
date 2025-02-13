using ap.nexus.agents.application.Services.ChatServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AP.Nexus.Module.Application
{
    public class ChatCachePruningService : BackgroundService
    {
        private readonly IChatMemoryStore _chatMemoryStore;
        private readonly ILogger<ChatCachePruningService> _logger;
        private readonly TimeSpan _pruneInterval;

        public ChatCachePruningService(
            IChatMemoryStore chatMemoryStore, 
            ILogger<ChatCachePruningService> logger,
            IConfiguration configuration)
        {
            _chatMemoryStore = chatMemoryStore;
            _logger = logger;

            _pruneInterval = TimeSpan.FromMinutes(configuration.GetValue<double>("ChatCache:PruneIntervalMinutes", 10));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ChatCachePruningService started.");

            using var timer = new PeriodicTimer(_pruneInterval);

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    _logger.LogDebug("Starting cache pruning.");

                    // Check if the cache is an InMemoryChatMemoryStore before pruning
                    if (_chatMemoryStore is InMemoryChatMemoryStore inMemoryStore)
                    {
                        var now = DateTime.UtcNow; // Get current time only once
                        await inMemoryStore.PruneInactiveThreadsAsync(TimeSpan.FromMinutes(30), now); // Pass current time
                        _logger.LogInformation("Pruned inactive threads at {Time}.", now);
                    }
                    else
                    {
                        _logger.LogDebug("Cache pruning is only supported for InMemoryChatMemoryStore. Skipping."); // More informative message
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during cache pruning.");
                }
            }

            _logger.LogInformation("ChatCachePruningService is stopping.");
        }
    }
}