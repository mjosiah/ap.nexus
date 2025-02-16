using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.application.Exceptions;
using ap.nexus.agents.domain.Entities;
using ap.nexus.core.data;
using Microsoft.EntityFrameworkCore;

namespace ap.nexus.agents.application.Services
{
    /// <summary>
    /// Service implementation for managing Agent entities.
    /// </summary>
    public class AgentService : IAgentService
    {
        private readonly IGenericRepository<Agent> _agentRepository;

        /// <summary>
        /// Constructs a new instance of AgentService with the provided repository.
        /// </summary>
        /// <param name="agentRepository">Repository for Agent entities.</param>
        public AgentService(IGenericRepository<Agent> agentRepository)
        {
            _agentRepository = agentRepository;
        }

        /// <summary>
        /// Creates a new agent based on the supplied request data.
        /// </summary>
        /// <param name="request">The agent creation data.</param>
        /// <returns>A DTO representing the created agent.</returns>
        /// <exception cref="FriendlyBusinessException">
        /// Thrown when the provided agent name is "Invalid".
        /// </exception>
        public async Task<AgentDto> CreateAgentAsync(CreateAgentRequest request)
        {
            // Business rule: Agent name cannot be "Invalid".
            if (string.Equals(request.Name, "Invalid", StringComparison.OrdinalIgnoreCase))
            {
                throw new FriendlyBusinessException("Agent name cannot be 'Invalid'.");
            }

            // Create a new agent entity
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

            // Persist the new agent entity
            await _agentRepository.AddAsync(agent);
            await _agentRepository.SaveChangesAsync();

            // Map the entity to its DTO representation and return it
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

        /// <summary>
        /// Deletes an existing agent identified by the provided external ID.
        /// </summary>
        /// <param name="agentExternalId">The external ID of the agent to delete.</param>
        /// <returns>A task that returns true if the deletion was successful.</returns>
        /// <exception cref="FriendlyBusinessException">
        /// Thrown when the agent with the provided external ID is not found.
        /// </exception>
        public async Task<bool> DeleteAgentAsync(Guid agentExternalId)
        {
            // Find the agent using its ExternalId
            var agent = await _agentRepository.Query()
                .FirstOrDefaultAsync(a => a.ExternalId == agentExternalId);

            if (agent == null)
            {
                throw new FriendlyBusinessException($"Agent with ExternalId '{agentExternalId}' was not found.");
            }

            // Remove the agent entity and save the changes
            await _agentRepository.DeleteAsync(agent);
            await _agentRepository.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves an agent by its external ID.
        /// </summary>
        /// <param name="agentExternalId">The external ID of the agent.</param>
        /// <returns>A task that returns the corresponding AgentDto.</returns>
        /// <exception cref="FriendlyBusinessException">
        /// Thrown when the agent with the provided external ID is not found.
        /// </exception>
        public async Task<AgentDto> GetAgentByExternalIdAsync(Guid agentExternalId)
        {
            var agent = await _agentRepository.Query()
                .FirstOrDefaultAsync(a => a.ExternalId == agentExternalId);

            if (agent == null)
            {
                throw new FriendlyBusinessException($"Agent with ExternalId '{agentExternalId}' was not found.");
            }

            // Deserialize the tools JSON to a list of ToolConfigurationDto objects
            var tools = System.Text.Json.JsonSerializer.Deserialize<List<ToolConfigurationDto>>(agent.ToolsJson)
                        ?? new List<ToolConfigurationDto>();

            // Map the entity to its DTO representation
            return new AgentDto
            {
                ExternalId = agent.ExternalId,
                Name = agent.Name,
                Description = agent.Description,
                Model = agent.Model,
                Instruction = agent.Instruction,
                ReasoningEffort = agent.ReasoningEffort,
                Tools = tools,
                Metadata = new Dictionary<string, string>(), // Adjust if Metadata is stored differently
                Scope = agent.Scope,
                ScopeExternalId = agent.ScopeExternalId
            };
        }

        /// <summary>
        /// Retrieves a paginated and sorted list of agents.
        /// </summary>
        /// <param name="input">Pagination and sorting parameters.</param>
        /// <returns>A paged result of AgentDto objects.</returns>
        public async Task<PagedResultDto<AgentDto>> GetAgentsAsync(PagedAndSortedResultRequestDto input)
        {
            // Start with the base query
            var query = _agentRepository.Query();

            // Apply sorting logic
            if (!string.IsNullOrWhiteSpace(input.Sorting))
            {
                // You can enhance this to support multiple sorting fields or different sort directions
                query = query.OrderBy(a => a.Name);
            }
            else
            {
                query = query.OrderBy(a => a.Id);
            }

            // Get the total count for pagination metadata
            var totalCount = await query.CountAsync();

            // Apply pagination (skip/take)
            var agents = await query.Skip(input.SkipCount)
                                    .Take(input.MaxResultCount)
                                    .ToListAsync();

            // Map the entity list to a list of DTOs
            var agentDtos = agents.Select(agent => new AgentDto
            {
                ExternalId = agent.ExternalId,
                Name = agent.Name,
                Description = agent.Description,
                Model = agent.Model,
                Instruction = agent.Instruction,
                ReasoningEffort = agent.ReasoningEffort,
                Tools = System.Text.Json.JsonSerializer.Deserialize<List<ToolConfigurationDto>>(agent.ToolsJson)
                        ?? new List<ToolConfigurationDto>(),
                Metadata = new Dictionary<string, string>(),
                Scope = agent.Scope,
                ScopeExternalId = agent.ScopeExternalId
            }).ToList();

            // Return the paged result
            return new PagedResultDto<AgentDto>
            {
                TotalCount = totalCount,
                Items = agentDtos
            };
        }

        /// <summary>
        /// Updates an existing agent with the new data provided in the request.
        /// </summary>
        /// <param name="agentExternalId">The external ID of the agent to update.</param>
        /// <param name="request">The update data.</param>
        /// <returns>A DTO representing the updated agent.</returns>
        /// <exception cref="FriendlyBusinessException">
        /// Thrown when the agent is not found or if the updated agent name is invalid.
        /// </exception>
        public async Task<AgentDto> UpdateAgentAsync(Guid agentExternalId, UpdateAgentRequest request)
        {
            // Locate the agent by its external ID
            var agent = await _agentRepository.Query()
                .FirstOrDefaultAsync(a => a.ExternalId == agentExternalId);

            if (agent == null)
            {
                throw new FriendlyBusinessException($"Agent with ExternalId '{agentExternalId}' was not found.");
            }

            // Business rule: Agent name cannot be "Invalid"
            if (!string.IsNullOrEmpty(request.Name) &&
                string.Equals(request.Name, "Invalid", StringComparison.OrdinalIgnoreCase))
            {
                throw new FriendlyBusinessException("Agent name cannot be 'Invalid'.");
            }

            // Update fields if provided (non-null values replace existing data)
            agent.Name = request.Name ?? agent.Name;
            agent.Description = request.Description ?? agent.Description;
            agent.Model = request.Model ?? agent.Model;
            agent.Instruction = request.Instructions ?? agent.Instruction;
            agent.ReasoningEffort = request.ReasoningEffort ?? agent.ReasoningEffort;

            

            // Update Tools if provided
            if (request.Tools != null)
            {
                agent.ToolsJson = System.Text.Json.JsonSerializer.Serialize(request.Tools);
            }

            // Persist the changes to the repository
            await _agentRepository.SaveChangesAsync();

            // Map the updated entity to its DTO
            var updatedTools = request.Tools ??
                               System.Text.Json.JsonSerializer.Deserialize<List<ToolConfigurationDto>>(agent.ToolsJson)
                               ?? new List<ToolConfigurationDto>();

            return new AgentDto
            {
                ExternalId = agent.ExternalId,
                Name = agent.Name,
                Description = agent.Description,
                Model = agent.Model,
                Instruction = agent.Instruction,
                ReasoningEffort = agent.ReasoningEffort,
                Tools = updatedTools,
                Metadata = request.Metadata ?? new Dictionary<string, string>(),
                Scope = agent.Scope,
                ScopeExternalId = agent.ScopeExternalId
            };
        }
    }
}
