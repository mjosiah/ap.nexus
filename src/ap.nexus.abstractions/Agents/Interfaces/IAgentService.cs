using ap.nexus.abstractions.Agents.DTOs;

namespace ap.nexus.abstractions.Agents.Interfaces
{
    public interface IAgentService
    {
        Task<AgentDto> CreateAgentAsync(CreateAgentRequest request);
        Task<AgentDto?> GetAgentByIdAsync(Guid agentId);
        Task<PagedResultDto<AgentDto>> GetAgentsAsync(PagedAndSortedResultRequestDto input);
        Task<bool> DeleteAgentAsync(Guid agentId);
        Task<AgentDto> UpdateAgentAsync(Guid agentId, UpdateAgentRequest request);
    }
}
