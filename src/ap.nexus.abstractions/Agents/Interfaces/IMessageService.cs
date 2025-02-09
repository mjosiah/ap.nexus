using Microsoft.SemanticKernel;

namespace ap.nexus.abstractions.Agents.Interfaces
{
    public interface IMessageService
    {
        /// <summary>
        /// Creates a new message in the data store.
        /// </summary>
        /// <param name="message">The ChatMessageContent to store.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task CreateMessageAsync(ChatMessageContent message);


        /// <summary>
        /// Retrieves messages by thread ExternalId.
        /// </summary>
        /// <param name="externalId">The ExternalId of the thread.</param>
        /// <returns>A Task representing the asynchronous operation, which returns a list of ChatMessageContent objects.</returns>
        Task<List<ChatMessageContent>> GetMessagesByThreadExternalIdAsync(Guid externalId);

        /// <summary>
        /// Retrieves a specific message by its ID. (Optional, but often useful)
        /// </summary>
        /// <param name="messageId">The ID of the message.</param>
        /// <returns>A Task representing the asynchronous operation, which returns the ChatMessageContent object or null if not found.</returns>
        Task<ChatMessageContent?> GetMessageByIdAsync(string messageId); // Consider nullable return


        /// <summary>
        /// Updates an existing message. (Optional, but useful for editing)
        /// </summary>
        /// <param name="message">The updated ChatMessageContent.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task UpdateMessageAsync(ChatMessageContent message);

        /// <summary>
        /// Deletes a message by its ID. (Optional)
        /// </summary>
        /// <param name="messageId">The ID of the message to delete.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task DeleteMessageAsync(string messageId);

        // Other methods as needed for your specific requirements,
        // e.g., pagination, searching, etc.
    }

}
