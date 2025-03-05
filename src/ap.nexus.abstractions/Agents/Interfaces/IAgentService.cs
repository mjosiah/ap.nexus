using ap.nexus.abstractions.Agents.DTOs;

namespace ap.nexus.abstractions.Agents.Interfaces
{
    public interface IAgentService
    {
        Task<Agent> CreateAgentAsync(CreateAgentRequest request);
        Task<Agent?> GetAgentByIdAsync(Guid agentId);
        Task<PagedResult<Agent>> GetAgentsAsync(PagedAndSortedResultRequest input);
        Task<bool> DeleteAgentAsync(Guid agentId);
        Task<Agent> UpdateAgentAsync(Guid agentId, UpdateAgentRequest request);
    }
}
