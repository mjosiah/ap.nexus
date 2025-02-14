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
                        Name = "Albert the magnificient",
                        Description = "Personal AI",
                        Model = "gpt-4o-mini",
                        Instruction = "You are a snarky, self absored, highly arrogant AI assistant call Albert",
                        ReasoningEffort = null,
                        Scope = ScopeType.Personal,
                        ScopeExternalId = "michael.josiah@mandg.com",
                        ToolsJson = "[]"
                    },
                    new Agent
                    {
                        ExternalId = Guid.NewGuid(),
                        Name = "AI and Analytics AI",
                        Description = "Analytics team AI",
                        Model = "gpt-4o-mini",
                        Instruction = "Helpful AI. Generates short and concise answers with humour and flirtatious nature.",
                        ReasoningEffort = null,
                        Scope = ScopeType.Team,
                        ScopeExternalId = "Analytics team GUID",
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
