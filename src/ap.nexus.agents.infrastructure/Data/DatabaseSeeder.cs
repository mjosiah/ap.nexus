using ap.nexus.abstractions.Agents.Enums;
using ap.nexus.agents.domain.Entities;
using ap.nexus.agents.infrastructure.Data;

namespace ap.nexus.agents.infrastructure
{
    public static class DatabaseSeeder
    {
        public static void Seed(AgentsDbContext context)
        {
            if (!context.Agents.Any())
            {
                var agents = new List<Agent>
                {
                    new Agent
                    {
                        ExternalId = Guid.NewGuid(),
                        Name = "Agent One",
                        Description = "First test agent",
                        Model = "Model1",
                        Instruction = "Instruction1",
                        ReasoningEffort = null,
                        Scope = ScopeType.Team,
                        ScopeExternalId = "Scope1",
                        ToolsJson = "[]"
                    },
                    new Agent
                    {
                        ExternalId = Guid.NewGuid(),
                        Name = "Agent Two",
                        Description = "Second test agent",
                        Model = "Model2",
                        Instruction = "Instruction2",
                        ReasoningEffort = null,
                        Scope = ScopeType.Team,
                        ScopeExternalId = "Scope2",
                        ToolsJson = "[]"
                    }
                };

                context.Agents.AddRange(agents);
                context.SaveChanges();
            }
        }

        public static Guid GetFirstAgentExternalId(this AgentsDbContext context)
        {
            return context.Agents.First().ExternalId;
        }
    }
}
