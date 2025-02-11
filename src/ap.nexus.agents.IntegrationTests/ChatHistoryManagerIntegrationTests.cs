using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.application.Services;
using ap.nexus.agents.domain.Entities;
using ap.nexus.agents.infrastructure.Data;
using ap.nexus.agents.infrastructure.Data.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly AgentsDbContext _context;

        public ChatHistoryManagerIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _chatHistoryManager = _fixture.ServiceProvider.GetRequiredService<IChatHistoryManager>();
            _threadService = _fixture.ServiceProvider.GetRequiredService<IThreadService>();
            _agentRepository = _fixture.ServiceProvider.GetRequiredService<IGenericRepository<Agent>>();
            _context = _fixture.ServiceProvider.GetRequiredService<AgentsDbContext>();
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
            var agentExternalId = _context.GetFirstAgentExternalId();
            var createRequest = new CreateChatThreadRequest
            {
                Title = "Test Prune Inactive Threads",
                AgentExternalId = agentExternalId,
                UserId = "TestUser"
            };
            var createdThread = await _chatHistoryManager.CreateThreadAsync(createRequest);

            await Task.Delay(TimeSpan.FromMinutes(35));

            _chatHistoryManager.PruneInactiveThreads();

            var result = await _chatHistoryManager.GetChatHistoryByExternalIdAsync(createdThread.ExternalId);
            result.Should().BeNull("because the thread should have been pruned due to inactivity.");
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