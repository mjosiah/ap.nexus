using System.Collections.Concurrent;
using ap.nexus.agents.infrastructure.DateTimeProviders;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ap.nexus.agents.application.Services.ChatServices
{
    public class InMemoryChatMemoryStore : IChatMemoryStore
    {
        private readonly ConcurrentDictionary<string, ChatThreadRecord> _store = new ConcurrentDictionary<string, ChatThreadRecord>();
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<InMemoryChatMemoryStore> _logger;


        public InMemoryChatMemoryStore(IDateTimeProvider dateTimeProvider, ILogger<InMemoryChatMemoryStore> logger)
        {
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public Task<ChatHistory?> GetChatHistoryAsync(Guid Id)
        {
            _store.TryGetValue(Id.ToString(), out var record);
            if (record != null)
            {
                record.UpdateLastAccessed();
                return Task.FromResult<ChatHistory?>(record.ChatHistory); // Return the actual reference!
            }
            return Task.FromResult<ChatHistory?>(null);
        }

        public Task SetChatHistoryAsync(Guid Id, ChatHistory chatHistory)
        {
            var key = Id.ToString();
            var record = new ChatThreadRecord(chatHistory);
            _store[key] = record;
            return Task.CompletedTask;
        }

        public Task RemoveChatHistoryAsync(Guid Id)
        {
            _store.TryRemove(Id.ToString(), out _);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Guid Id)
        {
            return Task.FromResult(_store.ContainsKey(Id.ToString()));
        }

        public int GetThreadCount()
        {
            return _store.Count;
        }

        public IEnumerable<string> GetInactiveThreads(TimeSpan inactivityThreshold, DateTime currentTime)
        {
            return _store.Where(kvp => currentTime - kvp.Value.LastAccessed > inactivityThreshold)
                        .Select(kvp => kvp.Key)
                        .ToList();
        }

        public async Task PruneInactiveThreadsAsync(TimeSpan inactivityThreshold, DateTime currentTime)
        {
            _logger.LogDebug("Starting pruning of inactive threads in InMemoryChatMemoryStore.");

            // Create a list of keys to remove to avoid modifying the _store dictionary while iterating.
            var keysToRemove = new List<string>();

            foreach (var kvp in _store)
            {
                var key = kvp.Key;
                var chatHistory = kvp.Value;

                if (chatHistory != null && chatHistory.LastAccessed.HasValue && currentTime - chatHistory.LastAccessed.Value > inactivityThreshold)
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                if (_store.TryRemove(key, out _))
                {
                    _logger.LogInformation("Pruned inactive thread {Key}", key);
                }
            }

            _logger.LogDebug("Finished pruning of inactive threads in InMemoryChatMemoryStore.");
        }

        
    }
}
