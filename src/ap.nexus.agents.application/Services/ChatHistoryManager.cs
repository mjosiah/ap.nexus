using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion; // For ChatHistory, ChatMessageContent, AuthorRole
using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.infrastructure.DateTimeProviders;

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
        Task<bool> MemoryContainsThread(Guid externalId);
    }

    public class ChatHistoryManager : IChatHistoryManager
    {
        private readonly IThreadService _threadService;
        private readonly IMessageService _messageService;
        private readonly IAgentService _agentService;
        private readonly ILogger<ChatHistoryManager> _logger;
        private readonly IChatMemoryStore _memoryStore;
        private static readonly TimeSpan InactivityThreshold = TimeSpan.FromMinutes(30);

        public ChatHistoryManager(
            IThreadService threadService,
            IMessageService messageService,
            IAgentService agentService,
            ILogger<ChatHistoryManager> logger,
            IChatMemoryStore memoryStore)
        {
            _threadService = threadService;
            _messageService = messageService;
            _agentService = agentService;
            _logger = logger;
            _memoryStore = memoryStore;
        }

        public async Task<ChatThreadDto> CreateThreadAsync(CreateChatThreadRequest request)
        {
            ChatThreadDto threadDto = await _threadService.CreateThreadAsync(request);
            var chatHistory = new ChatHistory();

            await _memoryStore.SetChatHistoryAsync(threadDto.ExternalId, chatHistory);

            _logger.LogInformation("Created new chat thread {ExternalId} and initialized chat history.", threadDto.ExternalId);
            return threadDto;
        }

        public async Task<ChatHistory?> GetChatHistoryByExternalIdAsync(Guid externalId)
        {
            ChatHistory? chatHistory = await _memoryStore.GetChatHistoryAsync(externalId);
            if (chatHistory != null)
            {
                _logger.LogInformation("Chat history for thread {ExternalId} found in memory store.", externalId);
                return chatHistory;
            }

            _logger.LogWarning("Chat history for thread {ExternalId} not found in memory store. Attempting to load from persistence.", externalId);

            ChatThreadDto? threadDto = await _threadService.GetThreadByExternalIdAsync(externalId);
            if (threadDto == null)
            {
                _logger.LogWarning("Thread {ExternalId} was not found in the database.", externalId);
                return null;
            }

            var storedMessages = await _messageService.GetMessagesByThreadExternalIdAsync(externalId);
            chatHistory = new ChatHistory();
            foreach (var storedMessage in storedMessages)
            {
                bool isUser = storedMessage.Role.Label.Equals("User", StringComparison.OrdinalIgnoreCase);
                chatHistory.Add(CreateChatMessageContent(storedMessage.Content, isUser));
            }

            await _memoryStore.SetChatHistoryAsync(externalId, chatHistory);
            return chatHistory;
        }

        public async Task AddUserMessageAsync(Guid externalId, string message)
        {
            await AddMessageAsync(externalId, message, isUser: true);
        }

        public async Task AddBotMessageAsync(Guid externalId, string message)
        {
            await AddMessageAsync(externalId, message, isUser: false);
        }

        public async Task AddMessageAsync(Guid externalId, string message, bool isUser)
        {
            var chatHistory = await GetChatHistoryByExternalIdAsync(externalId);
            if (chatHistory == null)
            {
                _logger.LogWarning("Attempted to add a message to thread {ExternalId} which does not exist.", externalId);
                throw new ArgumentException($"Thread with ExternalId {externalId} does not exist.");
            }

            ChatMessageContent chatMessageContent = CreateChatMessageContent(message, isUser);
            chatHistory.Add(chatMessageContent);

            await _memoryStore.SetChatHistoryAsync(externalId, chatHistory);
            await PersistMessageAsync(externalId, chatMessageContent);
        }

        public void ClearHistory(Guid externalId)
        {
            _memoryStore.RemoveChatHistoryAsync(externalId);
            _logger.LogInformation("Cleared chat history for thread {ExternalId}.", externalId);
        }

        

  

        public Task<bool> MemoryContainsThread(Guid externalId)
        {
            return _memoryStore.ExistsAsync(externalId);
        }

        private ChatMessageContent CreateChatMessageContent(string message, bool isUser)
        {
            return new ChatMessageContent(isUser ? AuthorRole.User : AuthorRole.Assistant, message);
        }

        private async Task PersistMessageAsync(Guid externalId, ChatMessageContent chatMessage)
        {
            try
            {
                await _messageService.AddMessageAsync(chatMessage, externalId);
                _logger.LogInformation("Persisted message for thread {ExternalId}.", externalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error persisting message for thread {ExternalId}.", externalId);
                throw;
            }
        }
    }
}
