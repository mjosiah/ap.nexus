using Microsoft.Extensions.Logging;
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
        Task<ChatHistory?> GetChatHistoryByIdAsync(Guid Id);
        Task<bool> ThreadExists(Guid Id);
        Task AddSystemMessageAsync(Guid Id, ChatMessageContent chatMessage);
        Task AddMessageAsync(Guid Id, ChatMessageContent chatMessage);
        void ClearHistory(Guid threadId);
        Task<bool> MemoryContainsThread(Guid Id);
        Task<ChatHistory> GetReducedChatHistoryAsync(Guid Id, IChatCompletionService chatService);
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

            await _memoryStore.SetChatHistoryAsync(threadDto.Id, chatHistory);

            _logger.LogInformation("Created new chat thread {Id} and initialized chat history.", threadDto.Id);
            return threadDto;
        }

        public async Task<ChatHistory?> GetChatHistoryByIdAsync(Guid Id)
        {
            ChatHistory? chatHistory = await _memoryStore.GetChatHistoryAsync(Id);
            if (chatHistory != null)
            {
                _logger.LogInformation("Chat history for thread {Id} found in memory store.", Id);
                return chatHistory;
            }

            _logger.LogWarning("Chat history for thread {Id} not found in memory store. Attempting to load from persistence.", Id);

            ChatThreadDto? threadDto = await _threadService.GetThreadByIdAsync(Id);
            if (threadDto == null)
            {
                _logger.LogWarning("Thread {Id} was not found in the database.", Id);
                return null;
            }

            var storedMessages = await _messageService.GetMessagesByThreadIdAsync(Id);
            chatHistory = new ChatHistory();
            foreach (var storedMessage in storedMessages)
            {
                var role = storedMessage.Role.Label.Equals("User", StringComparison.OrdinalIgnoreCase)
                    ? AuthorRole.User
                    : AuthorRole.Assistant;
                chatHistory.Add(new ChatMessageContent(role, storedMessage.Content));
            }

            await _memoryStore.SetChatHistoryAsync(Id, chatHistory);
            return chatHistory;
        }

        public async Task AddMessageAsync(Guid Id, ChatMessageContent chatMessage)
        {
            var chatHistory = await GetChatHistoryByIdAsync(Id);
            if (chatHistory == null)
            {
                _logger.LogWarning("Attempted to add a message to thread {Id} which does not exist.", Id);
                throw new ArgumentException($"Thread with Id {Id} does not exist.");
            }

            chatHistory.Add(chatMessage);

            await _memoryStore.SetChatHistoryAsync(Id, chatHistory);
            await PersistMessageAsync(Id, chatMessage);
        }

        public void ClearHistory(Guid Id)
        {
            _memoryStore.RemoveChatHistoryAsync(Id);
            _logger.LogInformation("Cleared chat history for thread {Id}.", Id);
        }

        public Task<bool> MemoryContainsThread(Guid Id)
        {
            return _memoryStore.ExistsAsync(Id);
        }

        public async Task AddSystemMessageAsync(Guid Id, ChatMessageContent chatMessage)
        {
            if (chatMessage.Role != AuthorRole.System)
            {
                _logger.LogWarning("Attempted to add a non-system message as a system message for thread {Id}.", Id);
                throw new ArgumentException("Only messages with AuthorRole.System can be added as system messages.");
            }

            var chatHistory = await GetChatHistoryByIdAsync(Id);
            if (chatHistory == null)
            {
                _logger.LogWarning("Chat history for thread {Id} not found.", Id);
                throw new ArgumentException($"Thread with Id {Id} does not exist.");
            }

            // Ensure there is no existing system message
            var existingSystemMessage = chatHistory.FirstOrDefault(m => m.Role == AuthorRole.System);
            if (existingSystemMessage != null)
            {
                _logger.LogInformation("System message already exists for thread {Id}. Skipping addition.", Id);
                return;
            }

            // Add the system message
            chatHistory.Insert(0, chatMessage);
            await _memoryStore.SetChatHistoryAsync(Id, chatHistory);
            _logger.LogInformation("System message added to thread {Id}.", Id);

        }

        public async Task<ChatHistory> GetReducedChatHistoryAsync(Guid Id, IChatCompletionService chatService)
        {
            // Retrieve full history  
            var fullHistory = await GetChatHistoryByIdAsync(Id);
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

        public async Task<bool> ThreadExists(Guid Id)
        {
            // First, check if the thread is in the in‑memory store.
            if (await _memoryStore.ExistsAsync(Id))
            {
                _logger.LogInformation("Thread {Id} exists in memory store.", Id);
                return true;
            }

            // If not in memory, check the database.
            if (await _threadService.ThreadIdExistsAsync(Id))
            {
                _logger.LogInformation("Thread {Id} exists in the database.", Id);
                return true;
            }

            _logger.LogWarning("Thread {Id} does not exist in memory or in the database.", Id);
            return false;
        }

        private async Task PersistMessageAsync(Guid Id, ChatMessageContent chatMessage)
        {
            try
            {
                await _messageService.AddMessageAsync(chatMessage, Id);
                _logger.LogInformation("Persisted message for thread {Id}.", Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error persisting message for thread {Id}.", Id);
                throw;
            }
        }
    }
}
