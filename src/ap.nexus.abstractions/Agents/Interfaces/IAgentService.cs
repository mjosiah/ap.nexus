using ap.nexus.abstractions.Agents.DTOs;

namespace ap.nexus.abstractions.Agents.Interfaces
{
    public interface IAgentService
    {
        Task<AgentDto> CreateAgentAsync(CreateAgentRequest request);
        Task<AgentDto?> GetAgentByExternalIdAsync(Guid agentExternalId);
        Task<PagedResultDto<AgentDto>> GetAllAgentsAsync(PagedAndSortedResultRequestDto input);
        Task<bool> DeleteAgentAsync(Guid agentExternalId);
        Task<AgentDto> UpdateAgentAsync(Guid agentExternalId, UpdateAgentRequest request);
    }
}
