using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.application.Exceptions;
using ap.nexus.agents.domain.Entities;
using ap.nexus.agents.infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ap.nexus.agents.application.Services
{
    public class AgentService : IAgentService
    {
        private readonly IGenericRepository<Agent> _agentRepository;

        public AgentService(IGenericRepository<Agent> agentRepository)
        {
            _agentRepository = agentRepository;
        }

        public async Task<AgentDto> CreateAgentAsync(CreateAgentRequest request)
        {
            // Business rule: Agent name cannot be "Invalid".
            if (request.Name.Equals("Invalid", StringComparison.OrdinalIgnoreCase))
            {
                throw new FriendlyBusinessException("Agent name cannot be 'Invalid'.");
            }

            var agent = new Agent
            {
                ExternalId = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Model = request.Model,
                Instruction = request.Instructions,
                ReasoningEffort = request.ReasoningEffort,
                Scope = request.Scope,
                ScopeExternalId = request.ScopeExternalId,
                ToolsJson = System.Text.Json.JsonSerializer.Serialize(request.Tools)
            };

            await _agentRepository.AddAsync(agent);
            await _agentRepository.SaveChangesAsync();

            return new AgentDto
            {
                ExternalId = agent.ExternalId,
                Name = agent.Name,
                Description = agent.Description,
                Model = agent.Model,
                Instruction = agent.Instruction,
                ReasoningEffort = agent.ReasoningEffort,
                Tools = request.Tools,
                Metadata = request.Metadata,
                Scope = agent.Scope,
                ScopeExternalId = agent.ScopeExternalId
            };
        }

        public Task<bool> DeleteAgentAsync(Guid agentExternalId)
        {
            throw new NotImplementedException();
        }

        public Task<AgentDto?> GetAgentByExternalIdAsync(Guid agentExternalId)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResultDto<AgentDto>> GetAllAgentsAsync(PagedAndSortedResultRequestDto input)
        {
            var query = _agentRepository.Query();

            if (!string.IsNullOrWhiteSpace(input.Sorting))
                query = query.OrderBy(a => a.Name);
            else
                query = query.OrderBy(a => a.Id);

            var totalCount = await query.CountAsync();
            var agents = await query.Skip(input.SkipCount)
                                    .Take(input.MaxResultCount)
                                    .ToListAsync();

            var agentDtos = agents.Select(agent => new AgentDto
            {
                ExternalId = agent.ExternalId,
                Name = agent.Name,
                Description = agent.Description,
                Model = agent.Model,
                Instruction = agent.Instruction,
                ReasoningEffort = agent.ReasoningEffort,
                Tools = System.Text.Json.JsonSerializer.Deserialize<List<ToolConfigurationDto>>(agent.ToolsJson) ?? new(),
                Metadata = new System.Collections.Generic.Dictionary<string, string>(),
                Scope = agent.Scope,
                ScopeExternalId = agent.ScopeExternalId
            }).ToList();

            return new PagedResultDto<AgentDto>
            {
                TotalCount = totalCount,
                Items = agentDtos
            };
        }

        public Task<AgentDto> UpdateAgentAsync(Guid agentExternalId, UpdateAgentRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
