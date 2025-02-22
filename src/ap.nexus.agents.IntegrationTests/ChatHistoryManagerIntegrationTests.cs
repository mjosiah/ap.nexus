using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.application.Services.ChatServices;
using ap.nexus.agents.domain.Entities;
using ap.nexus.agents.infrastructure.Data;
using ap.nexus.agents.infrastructure.DateTimeProviders;
using ap.nexus.core.data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion; // For ChatMessageContent and AuthorRole

namespace ap.nexus.agents.IntegrationTests
{
    public class ChatHistoryManagerIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly IChatHistoryManager _chatHistoryManager;
        private readonly IThreadService _threadService;
        private readonly IGenericRepository<Agent> _agentRepository;
        private readonly IGenericRepository<ChatThread> _chatThreadRepository;
        private readonly IChatMemoryStore _memoryStore;
        private readonly AgentsDbContext _context;

        public ChatHistoryManagerIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _chatHistoryManager = _fixture.ServiceProvider.GetRequiredService<IChatHistoryManager>();
            _threadService = _fixture.ServiceProvider.GetRequiredService<IThreadService>();
            _agentRepository = _fixture.ServiceProvider.GetRequiredService<IGenericRepository<Agent>>();
            _chatThreadRepository = _fixture.ServiceProvider.GetRequiredService<IGenericRepository<ChatThread>>();
            _context = _fixture.ServiceProvider.GetRequiredService<AgentsDbContext>();
            _memoryStore = _fixture.ServiceProvider.GetRequiredService<IChatMemoryStore>();
        }

        [Fact]
        public async Task CreateThreadAsync_ValidRequest_ReturnsChatThreadDtoWithCorrectProperties()
        {
            // Arrange
            var title = "Test Thread";

            // Act
            var createdThread = await CreateTestChatThreadAsync(title);

            // Assert
            createdThread.Should().NotBeNull();
            createdThread.Id.Should().NotBeEmpty();
            createdThread.Title.Should().Be(title);
        }

        [Fact]
        public async Task GetChatHistoryByIdAsync_EmptyHistory_ReturnsEmptyChatHistory()
        {
            // Arrange
            var title = "Test Get Empty History";
            var createdThread = await CreateTestChatThreadAsync(title);

            // Act
            var history = await _chatHistoryManager.GetChatHistoryByIdAsync(createdThread.Id);

            // Assert
            history.Should().NotBeNull();
            history.Should().BeEmpty("because no messages have been added yet");
        }

        [Fact]
        public async Task GetChatHistoryByIdAsync_HistoryInMemory_ReturnsCachedChatHistory()
        {
            // Arrange
            var title = "Test Get In-Memory History";
            var createdThread = await CreateTestChatThreadAsync(title);
            await AddTestUserMessageToThreadAsync(createdThread, "In-memory message");

            // Act
            var cachedHistory = await _chatHistoryManager.GetChatHistoryByIdAsync(createdThread.Id);

            // Assert
            cachedHistory.Should().NotBeNull();
            cachedHistory.Should().ContainSingle()
                .Which.Content.Should().Be("In-memory message");
        }

        [Fact]
        public async Task GetChatHistoryByIdAsync_HistoryNotInMemory_LoadsPersistedChatHistory()
        {
            // Arrange
            var title = "Test Load Persisted History";
            var createdThread = await CreateTestChatThreadAsync(title);
            await AddTestUserMessageToThreadAsync(createdThread, "Persisted message");

            // Simulate clearing the in-memory cache to force a load from persistence
            await _memoryStore.RemoveChatHistoryAsync(createdThread.Id);

            // Act
            var loadedHistory = await _chatHistoryManager.GetChatHistoryByIdAsync(createdThread.Id);

            // Assert
            loadedHistory.Should().NotBeNull();
            loadedHistory.Should().ContainSingle()
                .Which.Content.Should().Be("Persisted message");
        }

        [Fact]
        public async Task AddUserMessageAsync_ValidMessage_AddsMessageToChatHistory()
        {
            // Arrange
            var title = "Test Add User Message";
            var createdThread = await CreateTestChatThreadAsync(title);
            var userMessage = "Hello, this is a user message.";

            // Act
            await AddTestUserMessageToThreadAsync(createdThread, userMessage);
            var history = await _chatHistoryManager.GetChatHistoryByIdAsync(createdThread.Id);

            // Assert
            history.Should().NotBeNull();
            history.Should().ContainSingle()
                .Which.Content.Should().Be(userMessage);
        }

        [Fact]
        public async Task PruneInactiveThreads_InactiveThread_RemovesThreadFromMemory()
        {
            // Arrange
            var title = "Test Prune Inactive Threads";
            var createdThread = await CreateTestChatThreadAsync(title);

            // Get the TestDateTimeProvider
            var dateTimeProvider = _fixture.ServiceProvider.GetRequiredService<IDateTimeProvider>() as TestDateTimeProvider;
            dateTimeProvider.Should().NotBeNull();

            var pastTime = DateTime.UtcNow.AddDays(-1);
            dateTimeProvider.Now = pastTime; // Arrange: Set current time to the past

            // Access the chat history to create a record with the old LastAccessed timestamp
            await _chatHistoryManager.GetChatHistoryByIdAsync(createdThread.Id);

            // Act: Advance time and invoke pruning
            var currentTime = DateTime.UtcNow;
            dateTimeProvider.Now = currentTime; // Act: Set current time to now

            if (_memoryStore is InMemoryChatMemoryStore inMemoryStore)
            {
                await inMemoryStore.PruneInactiveThreadsAsync(TimeSpan.FromMinutes(30), dateTimeProvider.Now);
            }

            // Assert
            (await _memoryStore.ExistsAsync(createdThread.Id))
                .Should().BeFalse("because the history was pruned due to inactivity.");
        }

        [Fact]
        public async Task ClearHistory_ThreadExists_RemovesThreadFromMemory()
        {
            // Arrange
            var title = "Test Clear History";
            var createdThread = await CreateTestChatThreadAsync(title);
            await AddTestUserMessageToThreadAsync(createdThread, "First user message");
            await AddTestUserMessageToThreadAsync(createdThread, "Second user message");

            // Act
            _chatHistoryManager.ClearHistory(createdThread.Id);

            // Assert
            var exists = await _chatHistoryManager.MemoryContainsThread(createdThread.Id);
            exists.Should().BeFalse("because the history was cleared from memory.");
        }

        // Helper method to create a chat thread
        private async Task<ChatThreadDto> CreateTestChatThreadAsync(string title)
        {
            var agentId = _context.GetFirstAgentId();
            var request = new CreateChatThreadRequest
            {
                Title = title,
                AgentId = agentId,
                UserId = "TestUser"
            };
            return await _chatHistoryManager.CreateThreadAsync(request);
        }

        // Helper method to add a user message to a chat thread
        private async Task AddTestUserMessageToThreadAsync(ChatThreadDto thread, string message)
        {
            var messageContent = new ChatMessageContent(AuthorRole.User, message);
            await _chatHistoryManager.AddMessageAsync(thread.Id, messageContent);
        }
    }
}
