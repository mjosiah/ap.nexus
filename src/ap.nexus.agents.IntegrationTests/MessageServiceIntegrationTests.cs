using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.application.Services.ChatServices;
using ap.nexus.agents.domain.Entities;
using ap.nexus.agents.infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace ap.nexus.agents.IntegrationTests
{
    public class MessageServiceIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly IMessageService _messageService;
        private readonly IChatHistoryManager _chatHistoryManager;
        private readonly AgentsDbContext _context;

        public MessageServiceIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _messageService = _fixture.ServiceProvider.GetRequiredService<IMessageService>();
            _chatHistoryManager = _fixture.ServiceProvider.GetRequiredService<IChatHistoryManager>();
            _context = _fixture.ServiceProvider.GetRequiredService<AgentsDbContext>();
        }

        [Fact]
        public async Task AddMessageAsync_ValidMessage_PersistsMessageAndGeneratesId()
        {
            // Arrange
            var title = "Test Add Message";
            var thread = await CreateTestChatThreadAsync(title);
            var messageContent = new ChatMessageContent(AuthorRole.User, "Test message content");

            // Act
            await _messageService.AddMessageAsync(messageContent, thread.Id);

            // Assert
            var messages = await _messageService.GetMessagesByThreadIdAsync(thread.Id);
            messages.Should().NotBeEmpty();
            var addedMessage = messages.First();
            addedMessage.Content.Should().Be("Test message content");
        }

        [Fact]
        public async Task GetMessagesByThreadIdAsync_ReturnsMessagesForGivenThreadId()
        {
            // Arrange
            var title = "Test Get Messages By ThreadId";
            var thread = await CreateTestChatThreadAsync(title);
            var messageContent = new ChatMessageContent(AuthorRole.User, "Message for ThreadId test");
            await _messageService.AddMessageAsync(messageContent, thread.Id);

            var chatThreadEntity = await GetTestChatThreadByIdAsync(thread.Id);
            chatThreadEntity.Should().NotBeNull();

            // Act
            var messages = await _messageService.GetMessagesByThreadIdAsync(chatThreadEntity.Id);

            // Assert
            messages.Should().NotBeEmpty();
            messages.First().Content.Should().Be("Message for ThreadId test");
        }

        [Fact]
        public async Task GetMessageByIdAsync_ValidMessageId_ReturnsCorrectMessage()
        {
            // Arrange
            var title = "Test Get Message By Id";
            var thread = await CreateTestChatThreadAsync(title);
            var messageContent = new ChatMessageContent(AuthorRole.User, "Message to retrieve by ID");
            await _messageService.AddMessageAsync(messageContent, thread.Id);

            var chatMessage = await GetTestChatMessageAsync(threadId: thread.Id, content: "Message to retrieve by ID");
            chatMessage.Should().NotBeNull();

            // Act
            var retrievedMessage = await _messageService.GetMessageByIdAsync(chatMessage.Id.ToString());

            // Assert
            retrievedMessage.Should().NotBeNull();
            retrievedMessage.Content.Should().Be("Message to retrieve by ID");
        }

        [Fact]
        public async Task UpdateMessageAsync_ValidUpdate_UpdatesMessageContent()
        {
            // Arrange
            var title = "Test Update Message";
            var thread = await CreateTestChatThreadAsync(title);
            var originalContent = "Original message";
            var updatedContent = "Updated message content";
            var messageContent = new ChatMessageContent(AuthorRole.User, originalContent);
            await _messageService.AddMessageAsync(messageContent, thread.Id);

            var chatMessage = await GetTestChatMessageAsync(threadId: thread.Id, content: originalContent);
            chatMessage.Should().NotBeNull();

            // Act
            var updatedMessageContent = new ChatMessageContent(AuthorRole.User, updatedContent);
            await _messageService.UpdateMessageAsync(chatMessage.Id, updatedMessageContent);

            // Assert
            var updatedEntity = await _context.ChatMessages.FirstOrDefaultAsync(m => m.Id == chatMessage.Id);
            updatedEntity.Should().NotBeNull();
            updatedEntity.Content.Should().Be(updatedContent);
        }

        [Fact]
        public async Task DeleteMessageAsync_ValidMessageId_DeletesMessage()
        {
            // Arrange
            var title = "Test Delete Message";
            var thread = await CreateTestChatThreadAsync(title);
            var messageContent = new ChatMessageContent(AuthorRole.User, "Message to delete");
            await _messageService.AddMessageAsync(messageContent, thread.Id);

            var chatMessage = await GetTestChatMessageAsync(threadId: thread.Id, content: "Message to delete");
            chatMessage.Should().NotBeNull();

            // Act
            await _messageService.DeleteMessageAsync(chatMessage.Id.ToString());

            // Assert
            var deletedMessage = await _context.ChatMessages.FirstOrDefaultAsync(m => m.Id == chatMessage.Id);
            deletedMessage.Should().BeNull();
        }

        // Helper method to create a test chat thread using the ChatHistoryManager.
        private async Task<ChatThread> CreateTestChatThreadAsync(string title)
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

        // Generic helper method to retrieve a chat message based on multiple criteria.
        private async Task<ChatMessage?> GetTestChatMessageAsync(
            Guid? threadId = null,
            string? content = null,
            Guid? messageId = null)
        {
            var query = _context.ChatMessages.AsQueryable();

            
            if (threadId.HasValue)
            {
                query = query.Where(m => m.ChatThreadId == threadId);
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                query = query.Where(m => m.Content == content);
            }

            if (messageId.HasValue)
            {
                query = query.Where(m => m.Id == messageId);
            }

            return await query.FirstOrDefaultAsync();
        }

        [Fact]
        public async Task AddAndRetrieveMessage_WithPolymorphicItems_ShouldSerializeAndDeserializeCorrectly()
        {
            // Arrange: Create a test chat thread and a ChatMessageContent with TextContent and ImageContent.
            var title = "Test Polymorphic Serialization";
            var thread = await CreateTestChatThreadAsync(title);

            var messageContent = new ChatMessageContent(AuthorRole.User, "This is a polymorphic message.");
            var textContent = new TextContent { Text = "Sample text content" };
            var imageContent = new ImageContent { Uri = new Uri("http://example.com/image.jpg") };

            messageContent.Items.Add(textContent);
            messageContent.Items.Add(imageContent);

            // Act: Add the message to the database and retrieve it.
            await _messageService.AddMessageAsync(messageContent, thread.Id);
            var retrievedMessage = await _messageService.GetMessagesByThreadIdAsync(thread.Id);

            // Serialize the retrieved message to JSON for debugging.
            var retrievedJson = JsonSerializer.Serialize(retrievedMessage, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(retrievedJson);

            // Assert: Verify that the deserialization worked as expected.
            retrievedMessage.Should().NotBeEmpty();
            var deserializedMessage = retrievedMessage.First();

            deserializedMessage.Items.Should().HaveCount(3);  // Check if the count matches the expected 2 items

            var deserializedTextContent = deserializedMessage.Items.OfType<TextContent>().FirstOrDefault();
            deserializedTextContent.Should().NotBeNull();
            deserializedTextContent.Text.Should().Be("This is a polymorphic message.");

            var deserializedImageContent = deserializedMessage.Items.OfType<ImageContent>().FirstOrDefault();
            deserializedImageContent.Should().NotBeNull();
            deserializedImageContent.Uri.Should().Be(new Uri("http://example.com/image.jpg"));
        }



        // Helper method to get a chat thread entity by Id.
        private async Task<ChatThreadEntity?> GetTestChatThreadByIdAsync(Guid Id)
        {
            return await _context.ChatThreads.FirstOrDefaultAsync(t => t.Id == Id);
        }
    }
}
