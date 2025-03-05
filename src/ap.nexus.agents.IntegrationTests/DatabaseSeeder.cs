using ap.nexus.abstractions.Agents.Enums;
using ap.nexus.agents.domain.Entities;
using ap.nexus.agents.infrastructure.Data;

namespace ap.nexus.agents.IntegrationTests
{
    public static class TestDatabaseSeeder
    {
        public static void Seed(AgentsDbContext context)
        {
            if (!context.Agents.Any())
            {
                var agents = new List<AgentEntity>
                {
                    new AgentEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = "Agent One",
                        Description = "First test agent",
                        Model = "Model1",
                        Instruction = "Instruction1",
                        ReasoningEffort = null,
                        Scope = ScopeType.Team,
                        ScopeId = "Scope1",
                        Tools = "[]"
                    },
                    new AgentEntity
                    {
                        Id = Guid.NewGuid(),
                        Name = "Agent Two",
                        Description = "Second test agent",
                        Model = "Model2",
                        Instruction = "Instruction2",
                        ReasoningEffort = null,
                        Scope = ScopeType.Team,
                        ScopeId = "Scope2",
                        Tools = "[]"
                    }
                };

                context.Agents.AddRange(agents);
                context.SaveChanges();
            }
        }

        public static Guid GetFirstAgentId(this AgentsDbContext context)
        {
            return context.Agents.First().Id;
        }
    }
}
