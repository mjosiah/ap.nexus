using Microsoft.SemanticKernel;

namespace ap.nexus.abstractions.Agents.Interfaces
{
    public interface IMessageService
    {
        /// <summary>
        /// Creates a new message.
        /// </summary>
        /// <param name="message">The message content to store.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task AddMessageAsync(ChatMessageContent message, Guid threadId);

        /// <summary>
        /// Retrieves messages by thread's INTERNAL ID.
        /// </summary>
        /// <param name="threadId">The internal ID of the thread.</param>
        /// <returns>A Task representing the asynchronous operation, which returns a list of ChatMessageContent objects.</returns>
        Task<List<ChatMessageContent>> GetMessagesByThreadIdAsync(Guid threadId);


        /// <summary>
        /// Retrieves a specific message by its ID.
        /// </summary>
        /// <param name="messageId">The ID of the message.</param>
        /// <returns>A Task representing the asynchronous operation, which returns the ChatMessageContent object or null if not found.</returns>
        Task<ChatMessageContent?> GetMessageByIdAsync(string messageId);


        /// <summary>
        /// Updates an existing message.
        /// </summary>
        /// <param name="message">The updated message content.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task UpdateMessageAsync(Guid messageId, ChatMessageContent message);

        /// <summary>
        /// Deletes a message by its ID.
        /// </summary>
        /// <param name="messageId">The ID of the message to delete.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task DeleteMessageAsync(string messageId);

    }
}