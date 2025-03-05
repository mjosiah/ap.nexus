using ap.nexus.agents.api.contracts;
using ap.nexus.agents.apiclient;
using ap.nexus.agents.website.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace ap.nexus.agents.website.Services
{


    /// <summary>
    /// Service to interact with the Nexus chat API
    /// </summary>
    public class ChatApiService : IChatService
    {
        private readonly IChatApiClient _chatApiClient;
        private readonly StateContainer _stateContainer;
        private readonly ILogger<ChatApiService> _logger;

        public ChatApiService(
            IChatApiClient _chatApiClient,
            StateContainer stateContainer,
            ILogger<ChatApiService> logger)
        {
            this._chatApiClient = _chatApiClient;
            _stateContainer = stateContainer;
            _logger = logger;
        }

        /// <summary>
        /// Send a message to the chat API
        /// </summary>
        public async Task<MessageDto> SendMessageAsync(Guid agentId, string content, Guid? threadId = null)
        {
            try
            {
                _logger.LogInformation("Sending message to agent {AgentId} in thread {ThreadId}",
                    agentId, threadId);

                // Create chat message content
                var message = new ChatMessageContent(AuthorRole.User, content);

                // Create request
                var request = new ChatRequest
                {
                    AgentId = agentId,
                    ThreadId = threadId,
                    Message = message
                };

                // Send to API
                var response = await _chatApiClient.SendChatMessageAsync(request);

                // Convert user message to DTO
                var userMessageDto = new MessageDto
                {
                    Id = Guid.NewGuid(),
                    ChatSessionId = response.ThreadId,
                    Role = AuthorRole.User.Label,
                    SenderName = "User",
                    Timestamp = DateTime.UtcNow,
                    Items = new List<MessageContentItem>
                {
                    new MessageContentItem
                    {
                        ItemType = ContentItemType.Text,
                        Content = content
                    }
                }
                };

                // Convert response to DTO
                var responseDto = ConvertToMessageDto(response.Response, response.ThreadId);

                // Store in state
                _stateContainer.AddMessage(userMessageDto);
                _stateContainer.AddMessage(responseDto);

                // Update or create chat session if needed
                await UpdateChatSessionAsync(response.ThreadId, agentId);

                return responseDto;
            }
            catch (Refit.ApiException apiEx)
            {
                _logger.LogError(apiEx, "API error sending message to chat API");

                // Try to parse the error response
                var errorDto = await ParseErrorResponseAsync(apiEx);

                // Create an error message with the reason from the API
                var errorMessage = new MessageDto
                {
                    Id = Guid.NewGuid(),
                    ChatSessionId = threadId ?? Guid.Empty,
                    Role = AuthorRole.System.Label,
                    SenderName = "System",
                    Timestamp = DateTime.UtcNow,
                    Items = new List<MessageContentItem>
                {
                    new MessageContentItem
                    {
                        ItemType = ContentItemType.Text,
                        Content = $"Error: {errorDto.Reason ?? apiEx.Message}"
                    }
                }
                };

                // Store the error message in state
                _stateContainer.AddMessage(errorMessage);

                return errorMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to chat API");

                // Create generic error message
                var errorMessage = new MessageDto
                {
                    Id = Guid.NewGuid(),
                    ChatSessionId = threadId ?? Guid.Empty,
                    Role = AuthorRole.System.Label,
                    SenderName = "System",
                    Timestamp = DateTime.UtcNow,
                    Items = new List<MessageContentItem>
                {
                    new MessageContentItem
                    {
                        ItemType = ContentItemType.Text,
                        Content = $"Error sending message: {ex.Message}"
                    }
                }
                };

                // Store the error message in state
                _stateContainer.AddMessage(errorMessage);

                return errorMessage;
            }
        }

        /// <summary>
        /// Parse the API error response into an ApiErrorDto
        /// </summary>
        private async Task<ApiErrorDto> ParseErrorResponseAsync(Refit.ApiException apiException)
        {
            if (apiException.Content == null)
                return new ApiErrorDto { Code = (int)apiException.StatusCode, Reason = apiException.Message };

            try
            {
                var errorContent = apiException.Content;
                var errorDto = JsonSerializer.Deserialize<ApiErrorDto>(
                    errorContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return errorDto ?? new ApiErrorDto
                {
                    Code = (int)apiException.StatusCode,
                    Reason = apiException.Message
                };
            }
            catch
            {
                // If parsing fails, return a generic error
                return new ApiErrorDto
                {
                    Code = (int)apiException.StatusCode,
                    Reason = apiException.Message
                };
            }
        }

        /// <summary>
        /// Get message history for a thread
        /// </summary>
        public async Task<List<MessageDto>> GetMessageHistoryAsync(Guid threadId)
        {
            // In a real implementation, you would call an API endpoint to get the message history
            // For now, we'll return what's in the state container
            return _stateContainer.GetMessagesForChat(threadId);
        }

        /// <summary>
        /// Convert ChatMessageContent to MessageDto
        /// </summary>
        private MessageDto ConvertToMessageDto(ChatMessageContent message, Guid chatSessionId)
        {
            var messageDto = new MessageDto
            {
                Id = Guid.NewGuid(),
                ChatSessionId = chatSessionId,
                Role = message.Role.Label,
                Timestamp = DateTime.UtcNow,
                Items = new List<MessageContentItem>()
            };

            // Convert each item in the message
            foreach (var item in message.Items)
            {
                if (item is TextContent textContent)
                {
                    messageDto.Items.Add(new MessageContentItem
                    {
                        ItemType = ContentItemType.Text,
                        Content = textContent.Text
                    });
                }
                else if (item is ImageContent imageContent)
                {
                    messageDto.Items.Add(new MessageContentItem
                    {
                        ItemType = ContentItemType.Image,
                        Content = imageContent.Uri.ToString(),
                        Metadata = new Dictionary<string, string>
                        {
                            ["uri"] = imageContent.Uri.ToString()
                        }
                    });
                }
                // Add handlers for other content types as needed
            }

            // If the message has no items but has content, add it as a text item
            if (!messageDto.Items.Any() && !string.IsNullOrEmpty(message.Content))
            {
                messageDto.Items.Add(new MessageContentItem
                {
                    ItemType = ContentItemType.Text,
                    Content = message.Content
                });
            }

            return messageDto;
        }

        /// <summary>
        /// Update or create a chat session in the state container
        /// </summary>
        private async Task UpdateChatSessionAsync(Guid threadId, Guid agentId)
        {
            // Check if the chat session exists
            var existingSession = _stateContainer.ChatSessions.FirstOrDefault(s => s.Id == threadId);

            if (existingSession == null)
            {
                // Try to get agent name
                var agent = _stateContainer.AvailableAgents.FirstOrDefault(a => a.Id == agentId);
                var title = agent != null ? $"Chat with {agent.Name}" : $"Chat {threadId}";

                // Create a new chat session
                var newSession = new ChatSessionDto
                {
                    Id = threadId,
                    Title = title,
                    CreatedAt = DateTime.UtcNow,
                    LastActivityAt = DateTime.UtcNow,
                    IsWebSearchEnabled = _stateContainer.IsWebSearchEnabled,
                    IsDeepThinkingEnabled = _stateContainer.IsDeepThinkingEnabled
                };

                _stateContainer.ChatSessions.Add(newSession);
                _stateContainer.NotifyChatSessionsChanged();
            }
            else
            {
                // Update the existing session
                existingSession.LastActivityAt = DateTime.UtcNow;
                _stateContainer.NotifyChatSessionsChanged();
            }
        }

        /// <summary>
        /// Get available agents from the API
        /// </summary>
        public async Task<List<AgentDto>> GetAvailableAgentsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving available agents");

                // Create request with default paging
                var request = new PagedAndSortedResultRequestDto
                {
                    MaxResultCount = 50,
                    SkipCount = 0,
                    Sorting = "Name" // Sort by name
                };

                // Call API
                var response = await _chatApiClient.GetAgentsAsync(request);

                // Convert API agents to our AgentDto model
                var agents = response.Items.Select(a => new AgentDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Instruction = a.Instruction,
                    IsActive = true,
                    //AvatarUrl = a.AvatarUrl
                }).ToList();

                // Store in state
                _stateContainer.AvailableAgents = agents;

                return agents;
            }
            catch (Refit.ApiException apiEx)
            {
                _logger.LogError(apiEx, "API error retrieving agents. Status: {Status}, Content: {Content}",
                    apiEx.StatusCode, apiEx.Content);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agents");
                throw;
            }
        }
    }


    /// <summary>
    /// Interface for chat services
    /// </summary>
    public interface IChatService
    {
        Task<MessageDto> SendMessageAsync(Guid agentId, string content, Guid? threadId = null);
        Task<List<MessageDto>> GetMessageHistoryAsync(Guid threadId);
        Task<List<AgentDto>> GetAvailableAgentsAsync();
    }

    public class ApiErrorDto
    {
        public string Status { get; set; }
        public int Code { get; set; }
        public string Reason { get; set; }
        public string Note { get; set; }
    }
}
