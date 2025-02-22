using ap.nexus.abstractions.Agents.DTOs;

namespace ap.nexus.abstractions.Agents.Interfaces
{
    public interface IThreadService
    {
        Task<ChatThreadDto> CreateThreadAsync(CreateChatThreadRequest request);

        Task<ChatThreadDto?> GetThreadByIdAsync(Guid Id);

        Task<PagedResultDto<ChatThreadDto>> GetAllThreadsAsync(PagedAndSortedResultRequestDto input);
        Task DeleteThreadByIdAsync(Guid Id);
        Task<bool> ThreadIdExistsAsync(Guid Id);

    }
}
