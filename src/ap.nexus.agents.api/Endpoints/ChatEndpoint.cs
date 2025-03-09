using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.api.contracts;
using ap.nexus.agents.application.Services.ChatServices;
using FastEndpoints;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;
using System.Text.Json;

namespace ap.nexus.agents.api.Endpoints
{
    public class ChatEndpoint : Endpoint<ChatRequest, ChatResponse>
    {
        private readonly IAgentService _agentService;
        private readonly IThreadService _threadService;
        private readonly IMessageService _messageService;
        private readonly IKernelBuilder _kernelBuilder;
        private IChatCompletionService? _chatCompletionService;
        private readonly IConfiguration _configuration;
        private readonly IChatHistoryManager _chatHistoryManager;
        private readonly ILogger<ChatEndpoint> _logger;

        public ChatEndpoint(
            IAgentService agentService,
            IThreadService threadService,
            IMessageService messageService,
            IChatHistoryManager chatHistoryManager,
            IConfiguration configuration,
            ILogger<ChatEndpoint> logger)
        {
            _agentService = agentService;
            _threadService = threadService;
            _messageService = messageService;
            _configuration = configuration;
            _chatHistoryManager = chatHistoryManager;
            _logger = logger;
        }

        public override void Configure()
        {
            var request = CreateSampleChatRequest();

            Post("/chat");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Handles chat requests using Semantic Kernel agents.";
                s.Description = "Handles chat requests using Semantic Kernel agents with optional streaming support.";
                s.RequestExamples.Add(new FastEndpoints.RequestExample(JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true })));
            });
        }

        public override async Task HandleAsync(ChatRequest req, CancellationToken ct)
        {
            // Initialize chat components
            var setupResult = await InitializeChatAsync(req, ct);
            if (!setupResult.success) return; // Error already handled

            // Get initialized components
            var (kernel, threadId, agent) = setupResult.data;

            // Choose streaming or non-streaming mode
            if (req.UseStreaming)
            {
                await HandleStreamingChatAsync(kernel, threadId, agent, req, ct);
            }
            else
            {
                await HandleNonStreamingChatAsync(kernel, threadId, agent, req, ct);
            }
        }

        private async Task<(bool success, (Kernel kernel, Guid threadId, AgentDto agent) data)> InitializeChatAsync(ChatRequest req, CancellationToken ct)
        {
            try
            {
                // Retrieve the agent and ensure it exists
                var agent = await _agentService.GetAgentByIdAsync(req.AgentId);
                if (agent == null)
                {
                    AddError("Agent not found.");
                    ThrowIfAnyErrors();
                    return (false, (null, Guid.Empty, null));
                }

                // Validate OpenAI configuration
                var modelId = _configuration["OpenAI:ModelId"];
                var endpoint = _configuration["OpenAI:Endpoint"];
                var apiKey = _configuration["OpenAI:ApiKey"];
                if (string.IsNullOrEmpty(modelId) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
                {
                    AddError("OpenAI configuration is missing. Check your configuration.");
                    ThrowIfAnyErrors();
                    return (false, (null, Guid.Empty, null));
                }

                // Build the kernel with Azure OpenAI chat completion
                var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);
                builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
                Kernel kernel = builder.Build();
                _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

                // Determine the thread to use
                Guid threadId;
                if (req.ThreadId.HasValue)
                {
                    threadId = req.ThreadId.Value;
                }
                else
                {
                    threadId = Guid.NewGuid();
                }

                // If no chat history exists, create a new thread
                if (!await _chatHistoryManager.ThreadExists(threadId))
                {
                    var createThreadRequest = new CreateChatThreadRequest { AgentId = agent.Id };
                    var threadDto = await _chatHistoryManager.CreateThreadAsync(createThreadRequest);
                    threadId = threadDto.Id;
                }

                // Add system message
                var systemMessage = new ChatMessageContent(AuthorRole.System, agent.Instruction);
                await _chatHistoryManager.AddSystemMessageAsync(threadId, systemMessage);

                // Add the user's message
                await _chatHistoryManager.AddMessageAsync(threadId, req.Message);

                var agentDto = new AgentDto
                {
                    Id = agent.Id,
                    Name = agent.Name,
                    Instruction = agent.Instruction
                };

                return (true, (kernel, threadId, agentDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing chat");
                AddError($"Error initializing chat: {ex.Message}");
                ThrowIfAnyErrors();
                return (false, (null, Guid.Empty, null));
            }
        }

        private async Task HandleStreamingChatAsync(Kernel kernel, Guid threadId, AgentDto agent, ChatRequest req, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Starting streaming chat for thread {ThreadId}", threadId);

                // Set response content type for streaming
                HttpContext.Response.ContentType = "text/event-stream";
                HttpContext.Response.Headers.Add("Cache-Control", "no-cache");
                HttpContext.Response.Headers.Add("Connection", "keep-alive");

                // Get chat history
                var reducedMessages = await _chatHistoryManager.GetReducedChatHistoryAsync(threadId, _chatCompletionService);

                // Set execution settings
                var executionSettings = new OpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                };

                // Send start event with thread ID
                await WriteStreamEvent("start", new { threadId = threadId.ToString() }, ct);

                // Buffer for complete response
                var responseBuilder = new StringBuilder();

                // Start streaming
                var streamingResponse = _chatCompletionService.GetStreamingChatMessageContentsAsync(
                    reducedMessages,
                    executionSettings: executionSettings,
                    kernel: kernel
                );

                // Send each chunk to the client
                await foreach (var chunk in streamingResponse.WithCancellation(ct))
                {
                    if (!string.IsNullOrEmpty(chunk.Content))
                    {
                        responseBuilder.Append(chunk.Content);

                        // Send chunk to client
                        await WriteStreamEvent("chunk", new { content = chunk.Content, role = "assistant" }, ct);

                        // Ensure the response is sent immediately
                        await HttpContext.Response.Body.FlushAsync(ct);
                    }
                }

                // Create and persist the full response
                var fullResponseContent = responseBuilder.ToString();
                var responseMessage = new ChatMessageContent(AuthorRole.Assistant, fullResponseContent);



                await _chatHistoryManager.AddMessageAsync(threadId, responseMessage);

                // Send end event
                await WriteStreamEvent("end", new { threadId = threadId.ToString() }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during streaming chat");

                try
                {
                    // Try to send an error event
                    await WriteStreamEvent("error", new { message = $"Error: {ex.Message}" }, ct);
                }
                catch
                {
                    // Already in error state, just log
                    _logger.LogError("Failed to send error event");
                }
            }
        }

        private async Task HandleNonStreamingChatAsync(Kernel kernel, Guid threadId, AgentDto agent, ChatRequest req, CancellationToken ct)
        {
            try
            {
                // Get chat history
                var reducedMessages = await _chatHistoryManager.GetReducedChatHistoryAsync(threadId, _chatCompletionService);

                // Set execution settings
                var executionSettings = new OpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                };

                // Execute the chat completion
                var result = await _chatCompletionService.GetChatMessageContentAsync(
                    reducedMessages,
                    executionSettings: executionSettings,
                    kernel: kernel
                );

                // Create and persist the response message
#pragma warning disable SKEXP0001
                var responseMessage = new ChatMessageContent
                {
                    AuthorName = result.AuthorName ?? string.Empty,
                    Content = result.Content ?? string.Empty,
                    Items = result.Items,
                    Role = result.Role,
                    Metadata = result.Metadata,
                    ModelId = result.ModelId,
                };
#pragma warning restore SKEXP0001

                await _chatHistoryManager.AddMessageAsync(threadId, responseMessage);

                // Send the final response
                await SendAsync(new ChatResponse { Response = responseMessage, ThreadId = threadId }, cancellation: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in non-streaming chat");
                AddError($"Error: {ex.Message}");
                ThrowIfAnyErrors();
            }
        }

        private async Task WriteStreamEvent(string eventType, object data, CancellationToken ct)
        {
            var eventData = new
            {
                type = eventType,
                data
            };

            string json = JsonSerializer.Serialize(eventData);
            string eventText = $"data: {json}\n\n";

            byte[] bytes = Encoding.UTF8.GetBytes(eventText);
            await HttpContext.Response.Body.WriteAsync(bytes, ct);
            await HttpContext.Response.Body.FlushAsync(ct);
        }

        private ChatRequest CreateSampleChatRequest()
        {
            var messageContent = new ChatMessageContent(AuthorRole.User, "What is your name?");

            return new ChatRequest
            {
                AgentId = Guid.NewGuid(),
                ThreadId = Guid.NewGuid(),
                Message = messageContent,
                UseStreaming = false
            };
        }
    }
}