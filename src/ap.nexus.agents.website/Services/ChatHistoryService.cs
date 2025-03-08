using ap.nexus.agents.api.contracts;
using ap.nexus.agents.apiclient;
using ap.nexus.agents.website.Models;
using Microsoft.AspNetCore.Components;

namespace ap.nexus.agents.website.Services
{
    public class ChatHistoryService
    {
        private readonly IChatApiClient _apiClient;
        private readonly StateContainer _stateContainer;
        private readonly NavigationManager _navigationManager;
        private readonly ILogger<ChatHistoryService> _logger;

        public ChatHistoryService(
            IChatApiClient apiClient,
            StateContainer stateContainer,
            NavigationManager navigationManager,
            ILogger<ChatHistoryService> logger)
        {
            _apiClient = apiClient;
            _stateContainer = stateContainer;
            _navigationManager = navigationManager;
            _logger = logger;
        }

        /// <summary>
        /// Fetches chat threads for the current user and updates the state
        /// </summary>
        public async Task LoadChatHistoryAsync(string userId)
        {
            try
            {
                var request = new GetUserThreadsRequest
                {
                    UserId = userId,
                    MaxResultCount = 50,
                    SkipCount = 0,
                    Sorting = "CreatedAt DESC" // Most recent first
                };

                var result = await _apiClient.GetUserThreadsAsync(request);

                if (result?.Items != null)
                {
                    var chatSessions = result.Items.Select(thread => new ChatSessionDto
                    {
                        Id = thread.Id,
                        Title = thread.Title,
                        //CreatedAt = thread.CreatedAt,
                        //LastActivityAt = thread.LastModifiedAt ?? thread.CreatedAt,
                        UserId = Guid.Parse(thread.UserId)
                    }).ToList();

                    // Update the state using the property setter
                    _stateContainer.ChatSessions = chatSessions;
                    _logger.LogInformation("Loaded {Count} chat sessions for user {UserId}", chatSessions.Count, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chat history for user {UserId}", userId);
                // Consider showing error to user
            }
        }

        /// <summary>
        /// Navigates to a selected chat thread
        /// </summary>
        public void NavigateToChat(string chatId)
        {
            _navigationManager.NavigateTo($"/chat/{chatId}");
        }

        /// <summary>
        /// Creates a new chat and navigates to it
        /// </summary>
        public void StartNewChat()
        {
            _navigationManager.NavigateTo("/chat");
        }
    }
}