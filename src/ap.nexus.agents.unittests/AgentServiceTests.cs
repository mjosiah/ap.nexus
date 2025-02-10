using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Enums;
using ap.nexus.agents.application.Exceptions;
using ap.nexus.agents.application.Services;
using ap.nexus.agents.domain.Entities;
using ap.nexus.agents.infrastructure.Data.Repositories;
using FluentAssertions;
using Moq;

namespace ap.nexus.agents.unittests
{
    public class AgentServiceTests
    {
        private readonly Mock<IGenericRepository<Agent>> _agentRepositoryMock;
        private readonly AgentService _agentService;

        public AgentServiceTests()
        {
            _agentRepositoryMock = new Mock<IGenericRepository<Agent>>();
            _agentService = new AgentService(_agentRepositoryMock.Object);
        }


        [Fact]
        public async Task CreateAgentAsync_WhenNameIsInvalid_ShouldThrowFriendlyBusinessException()
        {
            // Arrange
            var request = new CreateAgentRequest
            {
                Name = "Invalid",
                Model = "ValidModel",
                Description = "Test description",
                Instructions = "Test instructions",
                Tools = new List<ToolConfigurationDto>(),
                Metadata = new Dictionary<string, string>(),
                ReasoningEffort = null,
                Scope = ScopeType.Team,
                ScopeExternalId = "Scope123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<FriendlyBusinessException>(() => _agentService.CreateAgentAsync(request));
        }

        [Fact]
        public async Task CreateAgentAsync_ValidRequest_ShouldReturnAgentDto()
        {
            // Arrange
            var request = new CreateAgentRequest
            {
                Name = "ValidName",
                Model = "ValidModel",
                Description = "Test description",
                Instructions = "Test instructions",
                Tools = new List<ToolConfigurationDto>(),
                Metadata = new Dictionary<string, string>(),
                ReasoningEffort = null,
                Scope = ScopeType.Team,
                ScopeExternalId = "Scope123"
            };

            _agentRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Agent>()))
                .ReturnsAsync((Agent agent) => agent);
            _agentRepositoryMock
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _agentService.CreateAgentAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);
            result.Model.Should().Be(request.Model);
        }

        [Fact]
        public async Task GetAllAgentsAsync_ShouldReturnPagedResult()
        {
            // Arrange
            var agents = new List<Agent>
            {
                new Agent { Id = 1, ExternalId = Guid.NewGuid(), Name = "Agent1", Description = "Desc1", Model = "Model1", Instruction = "Inst1", ToolsJson = "[]", Scope = ScopeType.Team, ScopeExternalId = "Scope1" },
                new Agent { Id = 2, ExternalId = Guid.NewGuid(), Name = "Agent2", Description = "Desc2", Model = "Model2", Instruction = "Inst2", ToolsJson = "[]", Scope = ScopeType.Personal, ScopeExternalId = "Scope2" }
            }.AsQueryable();

            _agentRepositoryMock.Setup(r => r.Query()).Returns(agents);

            var requestDto = new PagedAndSortedResultRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "Name"
            };

            // Act
            var result = await _agentService.GetAgentsAsync(requestDto);

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(agents.Count());
            result.Items.Should().HaveCount(agents.Count());
        }
    }
}
