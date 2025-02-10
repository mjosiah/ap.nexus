using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Enums;
using ap.nexus.abstractions.Agents.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ap.nexus.agents.IntegrationTests
{
    public class AgentIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly IAgentService _agentService;

        public AgentIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _agentService = _fixture.ServiceProvider.GetRequiredService<IAgentService>();
        }

        [Fact]
        public async Task GetAllAgents_ShouldReturnSeededAgents()
        {
            var requestDto = new PagedAndSortedResultRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "Name"
            };

            var result = await _agentService.GetAgentsAsync(requestDto);

            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty("because the database was seeded with test agents");
            result.TotalCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAgentAsync_ValidRequest_ShouldReturnAgentDto()
        {
            var request = new CreateAgentRequest
            {
                Name = "New Agent",
                Model = "TestModel",
                Description = "Test Description",
                Instructions = "Test Instructions",
                Tools = new System.Collections.Generic.List<ToolConfigurationDto>(),
                Metadata = new System.Collections.Generic.Dictionary<string, string>(),
                ReasoningEffort = null,
                Scope = ScopeType.Team,
                ScopeExternalId = "TestScope"
            };

            var result = await _agentService.CreateAgentAsync(request);

            result.Should().NotBeNull();
            result.Name.Should().Be("New Agent");
            result.Model.Should().Be("TestModel");
        }
    }
}
