using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json.Serialization;

namespace ap.nexus.agents.website.Models
{

    /// <summary>
    /// DTO representing a message in a chat session with support for multiple content types
    /// </summary>
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid ChatSessionId { get; set; }
        public string Role { get; set; }  // Added to store AuthorRole.System for errors
        public string SenderName { get; set; }
        public DateTime Timestamp { get; set; }
        public List<MessageContentItem> Items { get; set; } = new();

        /// <summary>
        /// Gets if the message is from the user
        /// </summary>
        public bool IsFromUser => Role == AuthorRole.User.Label;

        /// <summary>
        /// Gets if the message is a system message (like an error)
        /// </summary>
        public bool IsSystemMessage => Role == AuthorRole.System.Label;

        /// <summary>
        /// Gets the text content of all text items in this message
        /// </summary>
        public string TextContent
        {
            get
            {
                if (Items == null || !Items.Any())
                    return string.Empty;

                return string.Join("\n", Items
                    .Where(i => i.ItemType == ContentItemType.Text)
                    .Select(i => i.Content));
            }
        }
    }

    /// <summary>
    /// Represents a single content item within a message
    /// </summary>
    public class MessageContentItem
    {
        /// <summary>
        /// Type of content (Text, Image, Code, etc.)
        /// </summary>
        public ContentItemType ItemType { get; set; }

        /// <summary>
        /// The actual content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Additional metadata for the content item
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Types of content items that can be in a message
    /// </summary>
    public enum ContentItemType
    {
        Text,
        Image,
        Code,
        File,
        Citation,
        Equation,
        Markdown,
        Audio,
        Video,
        Embedding,
        Function,
        FunctionResult
    }

    /// <summary>
    /// Role of the message author
    /// </summary>
    //public enum AuthorRole
    //{
    //    User,
    //    Assistant,
    //    System
    //}

    /// <summary>
    /// Request to create a new message
    /// </summary>
    public class CreateMessageRequest
    {
        /// <summary>
        /// ID of the chat session this message belongs to
        /// </summary>
        public Guid ChatSessionId { get; set; }

        /// <summary>
        /// Text content of the message (for simple text messages)
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Role of the author
        /// </summary>
        public AuthorRole Role { get; set; } = AuthorRole.User;

        /// <summary>
        /// Name of the sender
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Complex content items (used instead of Content for multi-content messages)
        /// </summary>
        public List<MessageContentItem> Items { get; set; }
    }
}
