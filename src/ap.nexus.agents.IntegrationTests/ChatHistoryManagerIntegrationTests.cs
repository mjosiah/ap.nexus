using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.application.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ap.nexus.agents.IntegrationTests
{
    public class ChatHistoryManagerIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly IChatHistoryManager _chatHistoryManager;
        private readonly IThreadService _threadService;

        public ChatHistoryManagerIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _chatHistoryManager = _fixture.ServiceProvider.GetRequiredService<IChatHistoryManager>();
            _threadService = _fixture.ServiceProvider.GetRequiredService<IThreadService>();
        }

        [Fact]
        public async Task CreateThreadAsync_ValidRequest_ShouldReturnChatThreadDto()
        {
            var request = new CreateChatThreadRequest
            {
                Title = "Test Thread",
                AgentExternalId = Guid.NewGuid(),
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
            // Create a thread to work with
            var createRequest = new CreateChatThreadRequest
            {
                Title = "Test Get History",
                AgentExternalId = Guid.NewGuid(),
                UserId = "TestUser"
            };
            var createdThread = await _chatHistoryManager.CreateThreadAsync(createRequest);

            // Retrieve the thread by ExternalId
            var result = await _chatHistoryManager.GetChatHistoryByExternalIdAsync(createdThread.ExternalId);

            result.Should().NotBeNull();
            result.Should().BeEmpty("because no messages have been added yet");
        }

        [Fact]
        public async Task AddUserMessageAsync_ValidMessage_ShouldAddToChatHistory()
        {
            // Create a thread to work with
            var createRequest = new CreateChatThreadRequest
            {
                Title = "Test Add User Message",
                AgentExternalId = Guid.NewGuid(),
                UserId = "TestUser"
            };
            var createdThread = await _chatHistoryManager.CreateThreadAsync(createRequest);

            // Add a user message
            string userMessage = "Hello, this is a user message.";
            await _chatHistoryManager.AddUserMessageAsync(createdThread.ExternalId, userMessage);

            // Retrieve the thread and verify the message
            var result = await _chatHistoryManager.GetChatHistoryByExternalIdAsync(createdThread.ExternalId);
            result.Should().NotBeNull();
            result.Should().ContainSingle()
                .Which.Content.Should().Be(userMessage);
        }

        [Fact]
        public async Task AddBotMessageAsync_ValidMessage_ShouldAddToChatHistory()
        {
            // Create a thread to work with
            var createRequest = new CreateChatThreadRequest
            {
                Title = "Test Add Bot Message",
                AgentExternalId = Guid.NewGuid(),
                UserId = "TestUser"
            };
            var createdThread = await _chatHistoryManager.CreateThreadAsync(createRequest);

            // Add a bot message
            string botMessage = "Hello, this is a bot message.";
            await _chatHistoryManager.AddBotMessageAsync(createdThread.ExternalId, botMessage);

            // Retrieve the thread and verify the message
            var result = await _chatHistoryManager.GetChatHistoryByExternalIdAsync(createdThread.ExternalId);
            result.Should().NotBeNull();
            result.Should().ContainSingle()
                .Which.Content.Should().Be(botMessage);
        }

        [Fact]
        public async Task PruneInactiveThreads_ShouldRemoveInactiveThreads()
        {
            // Create a thread to work with
            var createRequest = new CreateChatThreadRequest
            {
                Title = "Test Prune Inactive Threads",
                AgentExternalId = Guid.NewGuid(),
                UserId = "TestUser"
            };
            var createdThread = await _chatHistoryManager.CreateThreadAsync(createRequest);

            // Simulate inactivity by waiting longer than the pruning threshold
            await Task.Delay(TimeSpan.FromMinutes(35));

            // Prune inactive threads
            _chatHistoryManager.PruneInactiveThreads();

            // Try to retrieve the thread after pruning
            var result = await _chatHistoryManager.GetChatHistoryByExternalIdAsync(createdThread.ExternalId);
            result.Should().BeNull("because the thread should have been pruned due to inactivity.");
        }

        [Fact]
        public async Task ClearHistory_ShouldRemoveAllMessages()
        {
            // Create a thread to work with
            var createRequest = new CreateChatThreadRequest
            {
                Title = "Test Clear History",
                AgentExternalId = Guid.NewGuid(),
                UserId = "TestUser"
            };
            var createdThread = await _chatHistoryManager.CreateThreadAsync(createRequest);

            // Add some messages
            await _chatHistoryManager.AddUserMessageAsync(createdThread.ExternalId, "First user message");
            await _chatHistoryManager.AddBotMessageAsync(createdThread.ExternalId, "First bot message");

            // Clear the history
            _chatHistoryManager.ClearHistory(createdThread.ExternalId);

            // Retrieve the thread and verify that no messages exist
            var result = await _chatHistoryManager.GetChatHistoryByExternalIdAsync(createdThread.ExternalId);
            result.Should().NotBeNull();
            result.Should().BeEmpty("because the history was cleared.");
        }
    }
}
