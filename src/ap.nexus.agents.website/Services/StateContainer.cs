using ap.nexus.agents.website.Models;

namespace ap.nexus.agents.website.Services
{

    /// <summary>
    /// Service that manages application state and provides notifications when state changes
    /// </summary>
    public class StateContainer
    {
        private readonly ILogger<StateContainer> _logger;

        public StateContainer(ILogger<StateContainer> logger)
        {
            _logger = logger;
        }

        #region Available Agents State

        private List<AgentDto> _availableAgents = new();

        // Public property with notification on change
        public List<AgentDto> AvailableAgents
        {
            get => _availableAgents;
            set
            {
                _availableAgents = value;
                NotifyAvailableAgentsChanged();
            }
        }

        // Event to notify subscribers when available agents change
        public event Action AvailableAgentsChanged;

        // Method to notify subscribers
        public void NotifyAvailableAgentsChanged()
        {
            _logger.LogDebug("Available agents changed. Count: {Count}", _availableAgents.Count);
            AvailableAgentsChanged?.Invoke();
        }

        #endregion

        #region Current Agent

        private Guid _currentAgentId;

        // Public property with notification on change
        public Guid CurrentAgentId
        {
            get => _currentAgentId;
            set
            {
                if (_currentAgentId != value)
                {
                    _logger.LogDebug("Current agent changed from {OldId} to {NewId}",
                        _currentAgentId, value);
                    _currentAgentId = value;
                    NotifyCurrentAgentChanged();
                }
            }
        }

        // Event to notify subscribers when current agent changes
        public event Action CurrentAgentChanged;

        // Method to notify subscribers
        public void NotifyCurrentAgentChanged()
        {
            CurrentAgentChanged?.Invoke();
        }

        // Method to get current agent object
        public AgentDto GetCurrentAgent()
        {
            return AvailableAgents.FirstOrDefault(a => a.Id == CurrentAgentId);
        }

        #endregion

        #region Chat Sessions State

        private List<ChatSessionDto> _chatSessions = new();

        // Public property with notification on change
        public List<ChatSessionDto> ChatSessions
        {
            get => _chatSessions;
            set
            {
                _chatSessions = value;
                NotifyChatSessionsChanged();
            }
        }

        public void AddChatSession(ChatSessionDto session)
        {
            // Check for duplicates first
            var existing = ChatSessions.FirstOrDefault(s => s.Id == session.Id);
            if (existing != null)
            {
                Console.WriteLine($"WARNING: Attempted to add duplicate chat session with ID {session.Id}, Title: {session.Title}");
                return; // Don't add duplicates
            }

            Console.WriteLine($"Adding chat session: {session.Id}, Title: {session.Title}");
            ChatSessions.Add(session);
            NotifyChatSessionsChanged();
        }

        public void RemoveChatSession(Guid sessionId)
        {
            var session = ChatSessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null)
            {
                Console.WriteLine($"Removing chat session: {session.Id}, Title: {session.Title}");
                ChatSessions.Remove(session);
                NotifyChatSessionsChanged();
            }
        }

        // Event to notify subscribers when chat sessions change
        public event Action ChatSessionsChanged;

        // Method to notify subscribers
        public void NotifyChatSessionsChanged()
        {
            _logger.LogDebug("Chat sessions changed. Count: {Count}", _chatSessions.Count);
            ChatSessionsChanged?.Invoke();
        }

        #endregion

        #region Current Chat Session

        private Guid _currentChatSessionId;

        // Public property with notification on change
        public Guid CurrentChatSessionId
        {
            get => _currentChatSessionId;
            set
            {
                if (_currentChatSessionId != value)
                {
                    _logger.LogDebug("Current chat session changed from {OldId} to {NewId}",
                        _currentChatSessionId, value);
                    _currentChatSessionId = value;
                    NotifyCurrentChatSessionChanged();
                }
            }
        }

        // Event to notify subscribers when current chat session changes
        public event Action CurrentChatSessionChanged;

        // Method to notify subscribers
        public void NotifyCurrentChatSessionChanged()
        {
            CurrentChatSessionChanged?.Invoke();
        }

        // Method to get current chat session object
        public ChatSessionDto GetCurrentChatSession()
        {
            return ChatSessions.FirstOrDefault(s => s.Id == CurrentChatSessionId);
        }

        #endregion

        #region Messages State

        // Store messages by chat session ID
        private readonly Dictionary<Guid, List<MessageDto>> _messagesByChat = new();

        // Event to notify subscribers when messages for a chat change
        public event Action<Guid> MessagesChanged;

        // Method to get messages for a specific chat
        public List<MessageDto> GetMessagesForChat(Guid chatId)
        {
            if (!_messagesByChat.ContainsKey(chatId))
            {
                _messagesByChat[chatId] = new List<MessageDto>();
            }

            return _messagesByChat[chatId];
        }

        // Method to get messages for current chat
        public List<MessageDto> GetMessagesForCurrentChat()
        {
            if (CurrentChatSessionId == Guid.Empty)
            {
                return new List<MessageDto>();
            }

            return GetMessagesForChat(CurrentChatSessionId);
        }

        // Method to set messages for a chat
        public void SetMessagesForChat(Guid chatId, List<MessageDto> messages)
        {
            _messagesByChat[chatId] = messages;
            _logger.LogDebug("Set {Count} messages for chat {ChatId}", messages.Count, chatId);
            MessagesChanged?.Invoke(chatId);
        }

        // Method to add a message to a chat
        public void AddMessage(MessageDto message)
        {
            if (message == null) return;

            if (!_messagesByChat.ContainsKey(message.ChatSessionId))
            {
                _messagesByChat[message.ChatSessionId] = new List<MessageDto>();
                _logger.LogDebug("Created new message collection for thread {ThreadId}", message.ChatSessionId);
            }

            _messagesByChat[message.ChatSessionId].Add(message);
            _logger.LogDebug("Added message to thread {ThreadId}: {Content}",
                message.ChatSessionId,
                message.TextContent.Substring(0, Math.Min(50, message.TextContent.Length)));

            MessagesChanged?.Invoke(message.ChatSessionId);

            // Update last activity time for the chat session
            var session = ChatSessions.FirstOrDefault(s => s.Id == message.ChatSessionId);
            if (session != null)
            {
                session.LastActivityAt = message.Timestamp;
                NotifyChatSessionsChanged();
            }
        }

        #endregion

        #region Feature Flags

        private bool _isWebSearchEnabled = true;
        private bool _isDeepThinkingEnabled = false;

        // Public properties with notification on change
        public bool IsWebSearchEnabled
        {
            get => _isWebSearchEnabled;
            set
            {
                if (_isWebSearchEnabled != value)
                {
                    _isWebSearchEnabled = value;
                    NotifyFeatureFlagsChanged();

                    // Update current chat session if applicable
                    if (CurrentChatSessionId != Guid.Empty)
                    {
                        var session = GetCurrentChatSession();
                        if (session != null)
                        {
                            session.IsWebSearchEnabled = value;
                        }
                    }
                }
            }
        }

        public bool IsDeepThinkingEnabled
        {
            get => _isDeepThinkingEnabled;
            set
            {
                if (_isDeepThinkingEnabled != value)
                {
                    _isDeepThinkingEnabled = value;
                    NotifyFeatureFlagsChanged();

                    // Update current chat session if applicable
                    if (CurrentChatSessionId != Guid.Empty)
                    {
                        var session = GetCurrentChatSession();
                        if (session != null)
                        {
                            session.IsDeepThinkingEnabled = value;
                        }
                    }
                }
            }
        }

        // Event to notify subscribers when feature flags change
        public event Action FeatureFlagsChanged;

        // Method to notify subscribers
        public void NotifyFeatureFlagsChanged()
        {
            _logger.LogDebug("Feature flags changed: WebSearch={WebSearch}, DeepThinking={DeepThinking}",
                _isWebSearchEnabled, _isDeepThinkingEnabled);
            FeatureFlagsChanged?.Invoke();
        }

        // Methods to toggle feature flags
        public void ToggleWebSearch()
        {
            IsWebSearchEnabled = !IsWebSearchEnabled;
        }

        public void ToggleDeepThinking()
        {
            IsDeepThinkingEnabled = !IsDeepThinkingEnabled;
        }

        #endregion

        #region UI State

        private bool _isSidebarCollapsed = false;

        // Public property with notification on change
        public bool IsSidebarCollapsed
        {
            get => _isSidebarCollapsed;
            set
            {
                if (_isSidebarCollapsed != value)
                {
                    _isSidebarCollapsed = value;
                    NotifySidebarStateChanged();
                }
            }
        }

        // Event to notify subscribers when sidebar state changes
        public event Action SidebarStateChanged;

        // Method to notify subscribers
        public void NotifySidebarStateChanged()
        {
            _logger.LogDebug("Sidebar state changed: Collapsed={Collapsed}", _isSidebarCollapsed);
            SidebarStateChanged?.Invoke();
        }

        // Method to toggle sidebar state
        public void ToggleSidebar()
        {
            IsSidebarCollapsed = !IsSidebarCollapsed;
        }

        #endregion

        #region Application State Reset

        // Method to reset application state
        public void ResetState()
        {
            AvailableAgents = new List<AgentDto>();
            ChatSessions = new List<ChatSessionDto>();
            _messagesByChat.Clear();
            CurrentAgentId = Guid.Empty;
            CurrentChatSessionId = Guid.Empty;
            _isWebSearchEnabled = true;
            _isDeepThinkingEnabled = false;
            _isSidebarCollapsed = false;

            _logger.LogInformation("Application state has been reset");
        }

        #endregion
    }

    /// <summary>
    /// DTO for agent information
    /// </summary>
    public class AgentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public string Instruction { get; set; }
    }
}
