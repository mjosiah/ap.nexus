using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion; // For ChatHistory, ChatMessageContent, AuthorRole
using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;

namespace ap.nexus.agents.application.Services
{
    /// <summary>
    /// Manages in‑memory chat histories and asynchronous persistence.
    /// Also prunes inactive threads from the cache.
    /// Leverages Semantic Kernel’s built‑in ChatHistory and ChatMessageContent models.
    /// </summary>
    public interface IChatHistoryManager
    {
        Task<ChatThreadDto> CreateThreadAsync(CreateChatThreadRequest request);
        Task<ChatHistory?> GetChatHistoryByExternalIdAsync(Guid externalId);
        Task AddUserMessageAsync(Guid threadExternalId, string message);
        Task AddBotMessageAsync(Guid threadExternalId, string message);
        void ClearHistory(Guid threadExternalId);
        void PruneInactiveThreads();
    }

    public class ChatHistoryManager : IChatHistoryManager
    {
        private readonly IThreadService _threadService;
        private readonly IMessageService _messageService;
        private readonly IAgentService _agentService;
        private readonly ILogger<ChatHistoryManager> _logger;

        // In‑memory store: we use the string representation of the ExternalId as the key.
        private readonly ConcurrentDictionary<string, ChatThreadRecord> _threads = new ConcurrentDictionary<string, ChatThreadRecord>();

        // Inactivity threshold for pruning (30 minutes here)
        private static readonly TimeSpan InactivityThreshold = TimeSpan.FromMinutes(30);

        public ChatHistoryManager(
            IThreadService threadService,
            IMessageService messageService,
            IAgentService agentService,
            ILogger<ChatHistoryManager> logger)
        {
            _threadService = threadService;
            _messageService = messageService;
            _agentService = agentService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new conversation thread.
        /// Persists the thread via IThreadService, caches an empty ChatHistory, and returns a ChatThreadDto.
        /// </summary>
        public async Task<ChatThreadDto> CreateThreadAsync(CreateChatThreadRequest request)
        {
            // Create the thread in persistence.
            ChatThreadDto threadDto = await _threadService.CreateThreadAsync(request);
            string key = threadDto.ExternalId.ToString();

            var chatHistory = new ChatHistory();
            var threadRecord = new ChatThreadRecord(chatHistory);
            if (!_threads.TryAdd(key, threadRecord))
            {
                _logger.LogError("Failed to add thread {ExternalId} to the in‑memory store.", threadDto.ExternalId);
                throw new Exception($"Unable to add thread {threadDto.ExternalId} to in‑memory store.");
            }
            return threadDto;
        }

        /// <summary>
        /// Retrieves the ChatHistory for a given thread by ExternalId.
        /// If not found in the cache, attempts to load it from the database.
        /// </summary>
        public async Task<ChatHistory?> GetChatHistoryByExternalIdAsync(Guid externalId)
        {
            string key = externalId.ToString();
            if (_threads.TryGetValue(key, out ChatThreadRecord record))
            {
                record.UpdateLastAccessed();
                return record.ChatHistory;
            }

            // Attempt to retrieve the thread DTO from persistence.
            ChatThreadDto? threadDto = await _threadService.GetThreadByExternalIdAsync(externalId);
            if (threadDto == null)
            {
                _logger.LogWarning("Thread {ExternalId} was not found in the database.", externalId);
                return null;
            }

            // Load messages from persistence.
            var storedMessages = await _messageService.GetMessagesByThreadExternalIdAsync(Guid.Parse(key));
            var chatHistory = new ChatHistory();
            foreach (var storedMessage in storedMessages)
            {
                bool isUser = storedMessage.Role.Label.Equals("User", StringComparison.OrdinalIgnoreCase);
                // Since ChatHistory's AddUserMessage/AddAssistantMessage return void, create the message manually.
                chatHistory.Add(CreateChatMessageContent(storedMessage.Content, isUser));
            }

            record = new ChatThreadRecord(chatHistory);
            _threads.TryAdd(key, record);
            return chatHistory;
        }

        public Task AddUserMessageAsync(Guid externalId, string message)
        {
            return AddMessageAsync(externalId, message, isUser: true);
        }

        public Task AddBotMessageAsync(Guid externalId, string message)
        {
            return AddMessageAsync(externalId, message, isUser: false);
        }

        /// <summary>
        /// Helper method to add a message to the thread identified by ExternalId.
        /// Since ChatHistory's methods do not return the message, we manually create a ChatMessageContent instance.
        /// </summary>
        private Task AddMessageAsync(Guid externalId, string message, bool isUser)
        {
            string key = externalId.ToString();
            if (!_threads.TryGetValue(key, out ChatThreadRecord record))
            {
                _logger.LogWarning("Attempted to add a message to thread {ExternalId} which is not in memory.", externalId);
                throw new ArgumentException($"Thread with ExternalId {externalId} does not exist.");
            }

            // Create the ChatMessageContent manually.
            ChatMessageContent chatMessageContent = CreateChatMessageContent(message, isUser);

            lock (record.Lock)
            {
                record.UpdateLastAccessed();
                record.ChatHistory.Add(chatMessageContent);
            }

            _ = PersistMessageAsync(key, chatMessageContent);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates a ChatMessageContent instance based on the provided message and role.
        /// </summary>
        private ChatMessageContent CreateChatMessageContent(string message, bool isUser)
        {
            return new ChatMessageContent(isUser ? AuthorRole.User : AuthorRole.Assistant, message);
        }

        private async Task PersistMessageAsync(string key, ChatMessageContent chatMessage)
        {
            try
            {
                await _messageService.CreateMessageAsync(chatMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving message to the database for thread {ExternalId}.", key);
            }
        }

        public void ClearHistory(Guid externalId)
        {
            string key = externalId.ToString();
            if (_threads.TryGetValue(key, out ChatThreadRecord record))
            {
                lock (record.Lock)
                {
                    record.ChatHistory.Clear();
                    record.UpdateLastAccessed();
                }
            }
        }

        public void PruneInactiveThreads()
        {
            DateTime now = DateTime.UtcNow;
            var keysToRemove = _threads.Keys.Where(k => (now - _threads[k].LastAccessed) > InactivityThreshold).ToList();
            foreach (var key in keysToRemove)
            {
                if (_threads.TryRemove(key, out _))
                {
                    _logger.LogInformation("Pruned inactive thread {ExternalId} from the cache.", key);
                }
            }
        }

        // Private nested class to hold each thread's state.
        private class ChatThreadRecord
        {
            public ChatHistory ChatHistory { get; }
            public object Lock { get; } = new object();
            public DateTime LastAccessed { get; private set; }

            public ChatThreadRecord(ChatHistory chatHistory)
            {
                ChatHistory = chatHistory;
                LastAccessed = DateTime.UtcNow;
            }

            public void UpdateLastAccessed() => LastAccessed = DateTime.UtcNow;
        }
    }
}
