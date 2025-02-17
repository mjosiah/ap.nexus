﻿using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion; // For ChatHistory, ChatMessageContent, AuthorRole
using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.infrastructure.DateTimeProviders;

namespace ap.nexus.agents.application.Services.ChatServices
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
        Task<bool> ThreadExists(Guid externalId);
        Task AddSystemMessageAsync(Guid externalId, ChatMessageContent chatMessage);
        Task AddMessageAsync(Guid externalId, ChatMessageContent chatMessage);
        void ClearHistory(Guid threadExternalId);
        Task<bool> MemoryContainsThread(Guid externalId);
        Task<ChatHistory> GetReducedChatHistoryAsync(Guid externalId, IChatCompletionService chatService);
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
                var role = storedMessage.Role.Label.Equals("User", StringComparison.OrdinalIgnoreCase)
                    ? AuthorRole.User
                    : AuthorRole.Assistant;
                chatHistory.Add(new ChatMessageContent(role, storedMessage.Content));
            }

            await _memoryStore.SetChatHistoryAsync(externalId, chatHistory);
            return chatHistory;
        }

        public async Task AddMessageAsync(Guid externalId, ChatMessageContent chatMessage)
        {
            var chatHistory = await GetChatHistoryByExternalIdAsync(externalId);
            if (chatHistory == null)
            {
                _logger.LogWarning("Attempted to add a message to thread {ExternalId} which does not exist.", externalId);
                throw new ArgumentException($"Thread with ExternalId {externalId} does not exist.");
            }

            chatHistory.Add(chatMessage);

            await _memoryStore.SetChatHistoryAsync(externalId, chatHistory);
            await PersistMessageAsync(externalId, chatMessage);
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

        public async Task AddSystemMessageAsync(Guid externalId, ChatMessageContent chatMessage)
        {
            if (chatMessage.Role != AuthorRole.System)
            {
                _logger.LogWarning("Attempted to add a non-system message as a system message for thread {ExternalId}.", externalId);
                throw new ArgumentException("Only messages with AuthorRole.System can be added as system messages.");
            }

            var chatHistory = await GetChatHistoryByExternalIdAsync(externalId);
            if (chatHistory == null)
            {
                _logger.LogWarning("Chat history for thread {ExternalId} not found.", externalId);
                throw new ArgumentException($"Thread with ExternalId {externalId} does not exist.");
            }

            // Ensure there is no existing system message
            var existingSystemMessage = chatHistory.FirstOrDefault(m => m.Role == AuthorRole.System);
            if (existingSystemMessage != null)
            {
                _logger.LogInformation("System message already exists for thread {ExternalId}. Skipping addition.", externalId);
                return;
            }

            // Add the system message
            chatHistory.Insert(0, chatMessage);
            await _memoryStore.SetChatHistoryAsync(externalId, chatHistory);
            _logger.LogInformation("System message added to thread {ExternalId}.", externalId);

        }

        public async Task<ChatHistory> GetReducedChatHistoryAsync(Guid externalId, IChatCompletionService chatService)
        {
            // Retrieve full history  
            var fullHistory = await GetChatHistoryByExternalIdAsync(externalId);
            if (fullHistory == null) return null;

            // Apply reduction logic
            #pragma warning disable SKEXP0001
            var reducer = new ChatHistorySummarizationReducer(chatService, 30, 50);
            #pragma warning restore SKEXP0001
            var reducedMessages = await reducer.ReduceAsync(fullHistory);

            // If no reduction was performed, return the full history.
            if (reducedMessages == null)
            {
                return fullHistory;
            }

            // Convert the IEnumerable<ChatMessageContent> to a ChatHistory.
            ChatHistory reducedHistory = new ChatHistory();
            reducedHistory.AddRange(reducedMessages);
            return reducedHistory;
        }

        public async Task<bool> ThreadExists(Guid externalId)
        {
            // First, check if the thread is in the in‑memory store.
            if (await _memoryStore.ExistsAsync(externalId))
            {
                _logger.LogInformation("Thread {ExternalId} exists in memory store.", externalId);
                return true;
            }

            // If not in memory, check the database.
            if (await _threadService.ThreadExternalIdExistsAsync(externalId))
            {
                _logger.LogInformation("Thread {ExternalId} exists in the database.", externalId);
                return true;
            }

            _logger.LogWarning("Thread {ExternalId} does not exist in memory or in the database.", externalId);
            return false;
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
