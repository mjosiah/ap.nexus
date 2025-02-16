using ap.nexus.abstractions.Agents.DTOs;

namespace ap.nexus.abstractions.Agents.Interfaces
{
    public interface IThreadService
    {
        Task<ChatThreadDto> CreateThreadAsync(CreateChatThreadRequest request);

        Task<ChatThreadDto?> GetThreadByExternalIdAsync(Guid externalId);

        Task<PagedResultDto<ChatThreadDto>> GetAllThreadsAsync(PagedAndSortedResultRequestDto input);
        Task DeleteThreadByExternalIdAsync(Guid externalId);
        Task<bool> ThreadExternalIdExistsAsync(Guid externalId);

    }
}
