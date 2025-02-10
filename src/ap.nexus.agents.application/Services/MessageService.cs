﻿using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.application.Exceptions; // Your custom exceptions
using ap.nexus.agents.domain.Entities;
using ap.nexus.agents.infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ap.nexus.agents.application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IGenericRepository<ChatMessage> _messageRepository;
        private readonly ILogger<MessageService> _logger;
        private readonly IGenericRepository<ChatThread> _chatThreadRepository;

        public MessageService(IGenericRepository<ChatMessage> messageRepository, ILogger<MessageService> logger, IGenericRepository<ChatThread> chatThreadRepository)
        {
            _messageRepository = messageRepository;
            _logger = logger;
            _chatThreadRepository = chatThreadRepository;
        }

        public async Task AddMessageAsync(ChatMessageContent message, Guid threadExternalId)
        {
            try
            {
                var chatThread = await _chatThreadRepository.Query().FirstOrDefaultAsync(t => t.ExternalId == threadExternalId);

                if (chatThread == null)
                {
                    _logger.LogWarning($"Thread with ExternalId {threadExternalId} not found.");
                    throw new FriendlyBusinessException($"Thread with ExternalId {threadExternalId} was not found.");
                }

                var chatMessage = new ChatMessage
                {
                    ExternalId = Guid.NewGuid(),
                    Content = message.Content,
                    ItemsJson = System.Text.Json.JsonSerializer.Serialize(message.Items),
                    ChatThreadId = chatThread.Id,
                    //UserId = message.UserId,
                    Role = message.Role == AuthorRole.User ? Role.User : Role.Assistant,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(message.Metadata),
                };

                await _messageRepository.AddAsync(chatMessage);
                await _messageRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding message.");
                throw new FriendlyBusinessException("Error adding message.");
            }
        }

        public async Task<List<ChatMessageContent>> GetMessagesByThreadExternalIdAsync(Guid threadExternalId)
        {
            try
            {
                var chatThread = await _chatThreadRepository.Query().FirstOrDefaultAsync(t => t.ExternalId == threadExternalId);
                if (chatThread == null)
                {
                    _logger.LogWarning($"Thread with ExternalId {threadExternalId} not found.");
                    throw new FriendlyBusinessException($"Thread with ExternalId {threadExternalId} was not found."); // Using FriendlyBusinessException
                }

                var messages = await _messageRepository.Query()
                    .Where(m => m.ChatThreadId == chatThread.Id)
                    .ToListAsync();

                return messages.Select(MapToChatMessageContent).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting messages by thread ExternalId: {threadExternalId}");
                throw new FriendlyBusinessException($"Error getting messages by thread ExternalId: {threadExternalId}");
            }
        }

        public async Task<List<ChatMessageContent>> GetMessagesByThreadIdAsync(int threadId)
        {
            try
            {
                var messages = await _messageRepository.Query()
                    .Where(m => m.ChatThreadId == threadId)
                    .ToListAsync();
                return messages.Select(MapToChatMessageContent).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting messages by thread ID: {threadId}");
                throw new FriendlyBusinessException($"Error getting messages by thread ID: {threadId}");
            }
        }

        public async Task<ChatMessageContent?> GetMessageByIdAsync(string messageId)
        {
            try
            {
                if (!Guid.TryParse(messageId, out Guid externalId))
                {
                    _logger.LogWarning($"Invalid messageId format: {messageId}");
                    return null;
                }

                var message = await _messageRepository.Query()
                    .FirstOrDefaultAsync(m => m.ExternalId == externalId);

                return message == null ? null : MapToChatMessageContent(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting message by ID: {messageId}");
                throw new FriendlyBusinessException($"Error getting message by ID: {messageId}");
            }
        }

        public async Task UpdateMessageAsync(int messageId, ChatMessageContent message)
        {
            try
            {
               
                var existingMessage = await _messageRepository.Query().FirstOrDefaultAsync(m => m.Id == messageId);

                if (existingMessage == null)
                {
                    _logger.LogWarning($"Message with ID {messageId} not found.");
                    throw new FriendlyBusinessException($"Message with ID {messageId} not found."); 
                }

                // Update the existing message with the new values.
                existingMessage.Content = message.Content;
                existingMessage.ItemsJson = System.Text.Json.JsonSerializer.Serialize(message.Items);
                existingMessage.MetadataJson = System.Text.Json.JsonSerializer.Serialize(message.Metadata);
                // ... Update other properties as needed ...

                await _messageRepository.UpdateAsync(existingMessage);
                await _messageRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating message with ID: {messageId}");
                throw new FriendlyBusinessException($"Error updating message with ID: {messageId}"); // Using FriendlyBusinessException
            }
        }

        public async Task DeleteMessageAsync(string messageId)
        {
            try
            {
                if (!Guid.TryParse(messageId, out Guid externalId))
                {
                    throw new ArgumentException("Invalid message ID format.");
                }

                var message = await _messageRepository.Query().FirstOrDefaultAsync(m => m.ExternalId == externalId);
                if (message == null)
                {
                    _logger.LogWarning($"Message with ID {messageId} not found.");
                    throw new FriendlyBusinessException($"Message with ID {messageId} not found.");
                }

                await _messageRepository.DeleteAsync(message);
                await _messageRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting message with ID: {messageId}");
                throw new FriendlyBusinessException($"Error deleting message with ID: {messageId}");
            }
        }

        private ChatMessageContent MapToChatMessageContent(ChatMessage message)
        {
            return new ChatMessageContent
            {
                Content = message.Content,
                Items = System.Text.Json.JsonSerializer.Deserialize<ChatMessageContentItemCollection>(message.ItemsJson) ?? new(),
                Role = message.Role == Role.User ? AuthorRole.User : AuthorRole.Assistant,
            };
        }
    }
}