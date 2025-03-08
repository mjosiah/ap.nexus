using ap.nexus.abstractions.Agents.DTOs;

namespace ap.nexus.abstractions.Agents.Interfaces
{
    public interface IThreadService
    {
        Task<ChatThread> CreateThreadAsync(CreateChatThreadRequest request);

        Task<ChatThread?> GetThreadByIdAsync(Guid Id);

        Task<PagedResult<ChatThread>> GetAllThreadsAsync(PagedAndSortedResultRequest input);
        Task DeleteThreadByIdAsync(Guid Id);
        Task<bool> ThreadIdExistsAsync(Guid Id);

    }
}
