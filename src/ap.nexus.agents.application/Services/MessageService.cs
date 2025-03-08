using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.application.Exceptions; // Your custom exceptions
using ap.nexus.agents.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using ap.nexus.core.data;

namespace ap.nexus.agents.application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IGenericRepository<ChatMessage> _messageRepository;
        private readonly ILogger<MessageService> _logger;
        private readonly IGenericRepository<ChatThreadEntity> _chatThreadRepository;

        public MessageService(IGenericRepository<ChatMessage> messageRepository, ILogger<MessageService> logger, IGenericRepository<ChatThreadEntity> chatThreadRepository)
        {
            _messageRepository = messageRepository;
            _logger = logger;
            _chatThreadRepository = chatThreadRepository;
        }

        public async Task AddMessageAsync(ChatMessageContent message, Guid threadId)
        {
            try
            {
                var chatThread = await _chatThreadRepository.Query().FirstOrDefaultAsync(t => t.Id == threadId);

                if (chatThread == null)
                {
                    _logger.LogWarning($"Thread with Id {threadId} not found.");
                    throw new FriendlyBusinessException($"Thread with Id {threadId} was not found.");
                }

                var chatMessage = new ChatMessage
                {
                    Content = message.Content,
                    Items = System.Text.Json.JsonSerializer.Serialize(message.Items),
                    ChatThreadId = chatThread.Id,
                    //UserId = message.UserId,
                    Role = message.Role == AuthorRole.User ? Role.User : (message.Role == AuthorRole.System ? Role.System : Role.Assistant),
                    MetaData = System.Text.Json.JsonSerializer.Serialize(message.Metadata),
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

        public async Task<List<ChatMessageContent>> GetMessagesByThreadIdAsync(Guid threadId)
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
                if (!Guid.TryParse(messageId, out Guid Id))
                {
                    _logger.LogWarning($"Invalid messageId format: {messageId}");
                    return null;
                }

                var message = await _messageRepository.Query()
                    .FirstOrDefaultAsync(m => m.Id == Id);

                return message == null ? null : MapToChatMessageContent(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting message by ID: {messageId}");
                throw new FriendlyBusinessException($"Error getting message by ID: {messageId}");
            }
        }

        public async Task UpdateMessageAsync(Guid messageId, ChatMessageContent message)
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
                existingMessage.Items = System.Text.Json.JsonSerializer.Serialize(message.Items);
                existingMessage.MetaData = System.Text.Json.JsonSerializer.Serialize(message.Metadata);
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
                if (!Guid.TryParse(messageId, out Guid Id))
                {
                    throw new ArgumentException("Invalid message ID format.");
                }

                var message = await _messageRepository.Query().FirstOrDefaultAsync(m => m.Id == Id);
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
                Items = System.Text.Json.JsonSerializer.Deserialize<ChatMessageContentItemCollection>(message.Items) ?? new(),
                Role = message.Role == Role.User
                        ? AuthorRole.User
                        : message.Role == Role.System
                            ? AuthorRole.System
                            : AuthorRole.Assistant,
            };
        }
    }
}