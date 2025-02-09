using ap.nexus.abstractions.Agents.DTOs;

namespace ap.nexus.abstractions.Agents.Interfaces
{
    public interface IThreadService
    {
        Task<ChatThreadDto> CreateThreadAsync(CreateChatThreadRequest request); // Takes the DTO

        Task<ChatThreadDto?> GetThreadByExternalIdAsync(Guid externalId); // Retrieves by ExternalId

        Task<PagedResultDto<ChatThreadDto>> GetAllThreadsAsync(PagedAndSortedResultRequestDto input);
        Task DeleteThreadByExternalIdAsync(Guid externalId); // Deletes by ExternalId

    }
}
