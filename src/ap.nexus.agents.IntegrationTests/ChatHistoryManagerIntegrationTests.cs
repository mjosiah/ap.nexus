using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.application.Services;
using ap.nexus.agents.domain.Entities;
using ap.nexus.agents.infrastructure.Data;
using ap.nexus.agents.infrastructure.Data.Repositories;
using ap.nexus.agents.infrastructure.DateTimeProviders;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Memory;
using System;
using System.Threading.Tasks;
using Xunit;

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
        public async Task CreateThreadAsync_ValidRequest_ShouldReturnChatThreadDto()
        {
            var agentExternalId = _context.GetFirstAgentExternalId();

            var request = new CreateChatThreadRequest
            {
                Title = "Test Thread",
                AgentExternalId = agentExternalId,
                UserId = "TestUser"
            };

            var result = await _chatHistoryManager.CreateThreadAsync(request);

            result.Should().NotBeNull();
            result.ExternalId.Should().NotBeEmpty();
            result.Title.Should().Be("Test Thread");
        }

        [Fact]
        public async Task GetChatHistoryByExternalIdAsync_ValidExternalId_ShouldReturnChatHistory()
        {
            var agentExternalId = _context.GetFirstAgentExternalId();

            var createRequest = new CreateChatThreadRequest
            {
                Title = "Test Get History",
                AgentExternalId = agentExternalId,
                UserId = "TestUser"
            };
            var createdThread = await _chatHistoryManager.CreateThreadAsync(createRequest);

            var result = await _chatHistoryManager.GetChatHistoryByExternalIdAsync(createdThread.ExternalId);

            result.Should().NotBeNull();
            result.Should().BeEmpty("because no messages have been added yet");
        }

        [Fact]
        public async Task AddUserMessageAsync_ValidMessage_ShouldAddToChatHistory()
        {
            var agentExternalId = _context.GetFirstAgentExternalId();
            var createRequest = new CreateChatThreadRequest
            {
                Title = "Test Add User Message",
                AgentExternalId = agentExternalId,
                UserId = "TestUser"
            };
            var createdThread = await _chatHistoryManager.CreateThreadAsync(createRequest);

            string userMessage = "Hello, this is a user message.";
            await _chatHistoryManager.AddUserMessageAsync(createdThread.ExternalId, userMessage);

            var result = await _chatHistoryManager.GetChatHistoryByExternalIdAsync(createdThread.ExternalId);
            result.Should().NotBeNull();
            result.Should().ContainSingle()
                .Which.Content.Should().Be(userMessage);
        }

        [Fact]
        public async Task AddBotMessageAsync_ValidMessage_ShouldAddToChatHistory()
        {
            var agentExternalId = _context.GetFirstAgentExternalId();
            var createRequest = new CreateChatThreadRequest
            {
                Title = "Test Add Bot Message",
                AgentExternalId = agentExternalId,
                UserId = "TestUser"
            };
            var createdThread = await _chatHistoryManager.CreateThreadAsync(createRequest);

            string botMessage = "Hello, this is a bot message.";
            await _chatHistoryManager.AddBotMessageAsync(createdThread.ExternalId, botMessage);

            var result = await _chatHistoryManager.GetChatHistoryByExternalIdAsync(createdThread.ExternalId);
            result.Should().NotBeNull();
            result.Should().ContainSingle()
                .Which.Content.Should().Be(botMessage);
        }

        [Fact]
        public async Task PruneInactiveThreads_ShouldRemoveInactiveThreads()
        {
            // 1. Arrange (Set up data in the past using TestDateTimeProvider)
            var agentExternalId = _context.GetFirstAgentExternalId();
            var createRequest = new CreateChatThreadRequest
            {
                Title = "Test Prune Inactive Threads",
                AgentExternalId = agentExternalId,
                UserId = "TestUser"
            };

            var createdThread = await _chatHistoryManager.CreateThreadAsync(createRequest);

            // Get the TestDateTimeProvider
            var dateTimeProvider = _fixture.ServiceProvider.GetRequiredService<IDateTimeProvider>() as TestDateTimeProvider;
            dateTimeProvider.Should().NotBeNull();

            var pastTime = DateTime.UtcNow.AddDays(-1);
            dateTimeProvider.Now = pastTime; // Set Now to past time

            // Access the chat history to create the ChatThreadRecord with the past LastAccessed
            await _chatHistoryManager.GetChatHistoryByExternalIdAsync(createdThread.ExternalId);


            // 2. Act (Advance time and call PruneInactiveThreads)
            var currentTime = DateTime.UtcNow;
            dateTimeProvider.Now = currentTime; // Set Now to current time

            if (_memoryStore is InMemoryChatMemoryStore inMemoryStore)
            {
                await inMemoryStore.PruneInactiveThreadsAsync(TimeSpan.FromMinutes(30), dateTimeProvider.Now);
            }

            // 3. Assert
            (await _memoryStore.ExistsAsync(createdThread.ExternalId)).Should().BeFalse();
        }

        [Fact]
        public async Task ClearHistory_ShouldRemoveAllMessages()
        {
            var agentExternalId = _context.GetFirstAgentExternalId();
            var createRequest = new CreateChatThreadRequest
            {
                Title = "Test Clear History",
                AgentExternalId = agentExternalId,
                UserId = "TestUser"
            };
            var createdThread = await _chatHistoryManager.CreateThreadAsync(createRequest);

            await _chatHistoryManager.AddUserMessageAsync(createdThread.ExternalId, "First user message");
            await _chatHistoryManager.AddBotMessageAsync(createdThread.ExternalId, "First bot message");

            _chatHistoryManager.ClearHistory(createdThread.ExternalId);

            var result = await _chatHistoryManager.GetChatHistoryByExternalIdAsync(createdThread.ExternalId);
            result.Should().NotBeNull();
            result.Should().BeEmpty("because the history was cleared.");
        }
    }
}