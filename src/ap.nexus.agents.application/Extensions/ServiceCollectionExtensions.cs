﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AP.Nexus.Module.Application;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.application.Services;
using ap.nexus.agents.application.Services.ChatServices;

namespace ap.nexus.agents.application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<ChatCachePruningService>();

            // Register application services.
            services.AddScoped<IAgentService, AgentService>();
            services.AddScoped<IThreadService, ThreadService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IChatHistoryManager, ChatHistoryManager>();
            services.AddSingleton<IChatMemoryStore, InMemoryChatMemoryStore>();

            return services;
        }
    }
}
