﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using StackExchange.Redis;
using System.Text.Json;

namespace ap.nexus.agents.application.Services.ChatServices
{
    public class RedisChatMemoryStore : IChatMemoryStore
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisChatMemoryStore> _logger;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly TimeSpan _defaultTtl;
        private readonly string _redisKeyPrefix;

        public RedisChatMemoryStore(
            IConnectionMultiplexer connectionMultiplexer,
            ILogger<RedisChatMemoryStore> logger,
            IConfiguration configuration)
        {
            _database = connectionMultiplexer.GetDatabase();
            _logger = logger;

            _defaultTtl = TimeSpan.FromMinutes(configuration.GetValue<double>("Redis:DefaultTTLMinutes", 30));
            _redisKeyPrefix = configuration.GetValue("Redis:KeyPrefix", "ChatHistory");

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
            };
        }

        public async Task<ChatHistory?> GetChatHistoryAsync(Guid Id)
        {
            string key = GetRedisKey(Id);
            try
            {
                var data = await _database.StringGetAsync(key);
                if (data.IsNullOrEmpty)
                {
                    return null;
                }

                var record = JsonSerializer.Deserialize<ChatThreadRecord>(data, _serializerOptions);

                if (record == null)
                {
                    _logger.LogError("Failed to deserialize ChatThreadRecord from Redis for key {Key}", key);
                    return null;
                }

                return record.ChatHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat history from Redis for key {Key}", key);
                return null; // Or re-throw if you prefer
            }
        }

        public async Task SetChatHistoryAsync(Guid Id, ChatHistory chatHistory)
        {
            string key = GetRedisKey(Id);
            try
            {
                var record = new ChatThreadRecord(chatHistory);
                string json = JsonSerializer.Serialize(record, _serializerOptions);
                await _database.StringSetAsync(key, json, _defaultTtl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting chat history in Redis for key {Key}", key);
                throw;
            }
        }

        public async Task RemoveChatHistoryAsync(Guid Id)
        {
            string key = GetRedisKey(Id);
            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing chat history from Redis for key {Key}", key);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid Id)
        {
            string key = GetRedisKey(Id);
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of chat history in Redis for key {Key}", key);
                return false;
            }
        }

        private string GetRedisKey(Guid Id)
        {
            return $"{_redisKeyPrefix}:{Id}";
        }

        public int GetThreadCount()
        {
            _logger.LogDebug("GetThreadCount called on RedisChatMemoryStore, but counting all keys is inefficient. Consider a separate counter.");
            return 0;
        }

        public IEnumerable<string> GetInactiveThreads(TimeSpan inactivityThreshold, DateTime currentTime)
        {
            _logger.LogDebug("GetInactiveThreads called on RedisChatMemoryStore");
            return Enumerable.Empty<string>();
        }

        public Task PruneInactiveThreadsAsync(TimeSpan inactivityThreshold, DateTime currentTime)
        {
            _logger.LogDebug("PruneInactiveThreadsAsync called on RedisChatMemoryStore, but pruning is handled by TTL. Returning CompletedTask.");
            return Task.CompletedTask;
        }
    }
}