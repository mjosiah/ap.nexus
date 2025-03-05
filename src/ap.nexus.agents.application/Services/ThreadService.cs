using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.domain.Entities;
using ap.nexus.core.data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ap.nexus.agents.application.Services
{
    public class ThreadService : IThreadService
    {
        private readonly IGenericRepository<ChatThread> _chatThreadRepository;
        private readonly IGenericRepository<Agent> _agentRepository;
        private readonly ILogger<ThreadService> _logger;

        public ThreadService(IGenericRepository<ChatThread> chatThreadRepository, IGenericRepository<Agent> agentRepository, ILogger<ThreadService> logger)
        {
            _chatThreadRepository = chatThreadRepository;
            _agentRepository = agentRepository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new chat thread based on the provided request.
        /// Generates a new Id, saves the thread, and returns a ChatThreadDto.
        /// </summary>
        public async Task<ChatThreadDto> CreateThreadAsync(CreateChatThreadRequest request)
        {
            try
            {
                var agent = await _agentRepository.FirstOrDefaultAsync(a => a.Id == request.AgentId);

                if (agent == null)
                {
                    throw new Exception($"Agent with Id {request.AgentId} not found."); 
                }

                var chatThread = new ChatThread
                {
                    AgentId = agent.Id,
                    Title = request.Title,
                    UserId = request.UserId
                };

                await _chatThreadRepository.AddAsync(chatThread);
                await _chatThreadRepository.SaveChangesAsync();

                _logger.LogInformation("Created chat thread with Id {Id} successfully.", chatThread.Id);

                return new ChatThreadDto
                {
                    Id = chatThread.Id,
                    Title = chatThread.Title,
                    AgentId = chatThread.AgentId,
                    UserId = chatThread.UserId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat thread for Title: {Title}", request.Title);
                throw new Exception("An error occurred while creating the chat thread.", ex);
            }
        }

        /// <summary>
        /// Retrieves a chat thread by its Id.
        /// Returns null if not found.
        /// </summary>
        public async Task<ChatThreadDto?> GetThreadByIdAsync(Guid Id)
        {
            try
            {

                var chatThread = await _chatThreadRepository
                    .Query()
                    .FirstOrDefaultAsync(ct => ct.Id == Id);

                if (chatThread == null)
                {
                    _logger.LogWarning("Chat thread with Id {Id} not found.", Id);
                    return null;
                }

                _logger.LogInformation("Retrieved chat thread with Id {Id} successfully.", Id);

                return new ChatThreadDto
                {
                    Id = chatThread.Id,
                    Title = chatThread.Title,
                    AgentId = chatThread.AgentId,
                    UserId = chatThread.UserId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat thread with Id: {Id}", Id);
                throw new Exception("An error occurred while retrieving the chat thread.", ex);
            }
        }

        /// <summary>
        /// Retrieves all chat threads in a paged format.
        /// </summary>
        public async Task<PagedResult<ChatThreadDto>> GetAllThreadsAsync(PagedAndSortedResultRequest input)
        {
            try
            {
                var query = _chatThreadRepository.Query();

                // Use provided sorting or default to ordering by Id.
                if (!string.IsNullOrWhiteSpace(input.Sorting))
                {
                    query = query.OrderBy(ct => ct.Title);
                }
                else
                {
                    query = query.OrderBy(ct => ct.Id);
                }

                var totalCount = await query.CountAsync();

                var threads = await query
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .ToListAsync();

                var threadDtos = threads.Select(ct => new ChatThreadDto
                {
                    Id = ct.Id,
                    Title = ct.Title,
                    AgentId = ct.AgentId,
                    UserId = ct.UserId
                }).ToList();

                _logger.LogInformation("Retrieved {Count} chat threads.", threadDtos.Count);

                return new PagedResult<ChatThreadDto>
                {
                    TotalCount = totalCount,
                    Items = threadDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all chat threads.");
                throw new Exception("An error occurred while retrieving chat threads.", ex);
            }
        }

        /// <summary>
        /// Deletes a chat thread by its Id.
        /// Throws an exception if the thread is not found.
        /// </summary>
        public async Task DeleteThreadByIdAsync(Guid Id)
        {
            try
            {
                var chatThread = await _chatThreadRepository
                    .Query()
                    .FirstOrDefaultAsync(ct => ct.Id == Id);

                if (chatThread == null)
                {
                    _logger.LogWarning("Attempted to delete chat thread with Id {Id} but it was not found.", Id);
                    throw new Exception($"ChatThread with Id {Id} was not found.");
                }

                await _chatThreadRepository.DeleteAsync(chatThread);
                await _chatThreadRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted chat thread with Id {Id}.", Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chat thread with Id {Id}.", Id);
                throw new Exception("An error occurred while deleting the chat thread.", ex);
            }
        }

        public async Task<bool> ThreadIdExistsAsync(Guid Id)
        {
            try
            {
                return await _chatThreadRepository.Query().AnyAsync(ct => ct.Id == Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of chat thread with Id {Id}", Id);
                throw;
            }
        }
    }
}
