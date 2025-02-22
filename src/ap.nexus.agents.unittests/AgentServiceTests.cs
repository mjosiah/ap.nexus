using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Enums;
using ap.nexus.agents.application.Exceptions;
using ap.nexus.agents.application.Services;
using ap.nexus.agents.domain.Entities;
using ap.nexus.core.data;
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
                ScopeId = "Scope123"
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
                ScopeId = "Scope123"
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

       
    }
}
