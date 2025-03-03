﻿using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json.Serialization;

namespace ap.nexus.agents.website.Models
{

    /// <summary>
    /// DTO representing a message in a chat session with support for multiple content types
    /// </summary>
    public class MessageDto
    {
        /// <summary>
        /// Unique identifier for the message
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID of the chat session this message belongs to
        /// </summary>
        public Guid ChatSessionId { get; set; }

        /// <summary>
        /// Role of the message author (User, Assistant, System)
        /// </summary>
        public AuthorRole Role { get; set; }

        /// <summary>
        /// Name of the sender
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// When the message was sent
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Collection of content items in this message
        /// </summary>
        public List<MessageContentItem> Items { get; set; } = new();

        /// <summary>
        /// Convenience property to check if the message is from the user
        /// </summary>
        [JsonIgnore]
        public bool IsFromUser => Role == AuthorRole.User;

        /// <summary>
        /// Convenience property to get the text content of the message
        /// </summary>
        [JsonIgnore]
        public string TextContent => string.Join("\n",
            Items.Where(i => i.ItemType == ContentItemType.Text)
                 .Select(i => i.Content));

        /// <summary>
        /// Creates a simple text message
        /// </summary>
        public static MessageDto CreateTextMessage(
            Guid chatSessionId,
            string content,
            AuthorRole role,
            string senderName = null)
        {
            return new MessageDto
            {
                Id = Guid.NewGuid(),
                ChatSessionId = chatSessionId,
                Role = role,
                SenderName = senderName ?? role.ToString(),
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
