using ap.nexus.agents.application.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AP.Nexus.Module.Application
{
    public class ChatCachePruningService : BackgroundService
    {
        private readonly ChatHistoryManager _chatHistoryManager;
        private readonly ILogger<ChatCachePruningService> _logger;
        private readonly TimeSpan _pruneInterval;

        public ChatCachePruningService(ChatHistoryManager chatHistoryManager, ILogger<ChatCachePruningService> logger)
        {
            _chatHistoryManager = chatHistoryManager;
            _logger = logger;
            // Set the interval between pruning operations (adjust as necessary)
            _pruneInterval = TimeSpan.FromMinutes(10);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ChatCachePruningService started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _chatHistoryManager.PruneInactiveThreads();
                    _logger.LogInformation("Pruned inactive threads at {Time}.", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during cache pruning.");
                }

                try
                {
                    await Task.Delay(_pruneInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // graceful shutdown
                    break;
                }
            }
            _logger.LogInformation("ChatCachePruningService is stopping.");
        }
    }
}
