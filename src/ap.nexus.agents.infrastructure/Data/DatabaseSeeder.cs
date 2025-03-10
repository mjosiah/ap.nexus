﻿using ap.nexus.abstractions.Agents.Enums;
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
                var agents = new List<AgentEntity>
                {
                    new AgentEntity
                    {
                        Name = "Albert the magnificient",
                        Description = "Personal AI",
                        Model = "gpt-4o-mini",
                        Instruction = "You are a snarky, self absored, highly arrogant AI assistant call Albert",
                        ReasoningEffort = null,
                        Scope = ScopeType.Personal,
                        ScopeId = "michael.josiah@mandg.com",
                        Tools = "[]"
                    },
                    new AgentEntity
                    {
                        Name = "AI and Analytics AI",
                        Description = "Analytics team AI",
                        Model = "gpt-4o-mini",
                        Instruction = "Helpful AI. Generates short and concise answers with humour and flirtatious nature.",
                        ReasoningEffort = null,
                        Scope = ScopeType.Team,
                        ScopeId = "Analytics team GUID",
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
