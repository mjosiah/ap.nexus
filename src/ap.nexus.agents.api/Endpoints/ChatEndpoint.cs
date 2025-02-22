using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using ap.nexus.agents.application.Services.ChatServices;
using FastEndpoints;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ap.nexus.agents.api.Endpoints
{
    public class ChatRequest
    {
        [Required]
        public Guid AgentId { get; set; }
        public Guid? ThreadId { get; set; }
        [Required]
        public ChatMessageContent Message { get; set; }
    }

    public class ChatResponse
    {
        public ChatMessageContent Response { get; set; } = new();
        public Guid ThreadId { get; set; }
    }
    public class ChatEndpoint : Endpoint<ChatRequest, ChatResponse>
    {
        private readonly IAgentService _agentService;
        private readonly IThreadService _threadService;
        private readonly IMessageService _messageService;
        private readonly IKernelBuilder _kernelBuilder;
        private IChatCompletionService? _chatCompletionService;
        private readonly IConfiguration _configuration;
        private readonly IChatHistoryManager _chatHistoryManager;

        public ChatEndpoint(IAgentService agentService, IThreadService threadService, IMessageService messageService, IChatHistoryManager chatHistoryManager, IConfiguration configuration)
        {
            _agentService = agentService;
            _threadService = threadService;
            _messageService = messageService;
            _configuration = configuration;
            _chatHistoryManager = chatHistoryManager;
        }

        public override void Configure()
        {
            var request = CreateSampleChatRequest();

            Post("/chat");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Handles chat requests using Semantic Kernel agents.";
                s.Description = "Handles chat requests using Semantic Kernel agents.";
                s.RequestExamples.Add(new FastEndpoints.RequestExample(JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true })));
            });
        }

        public override async Task HandleAsync(ChatRequest req, CancellationToken ct)
        {
            // Retrieve the agent and ensure it exists.
            var agent = await _agentService.GetAgentByIdAsync(req.AgentId);
            if (agent == null)
            {
                AddError("Agent not found.");
                ThrowIfAnyErrors();
                return;
            }

            // Validate OpenAI configuration.
            var modelId = _configuration["OpenAI:ModelId"];
            var endpoint = _configuration["OpenAI:Endpoint"];
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(modelId) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                AddError("OpenAI configuration is missing. Check your configuration.");
                ThrowIfAnyErrors();
                return;
            }

            // Build the kernel with Azure OpenAI chat completion.
            var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);
            builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
            Kernel kernel = builder.Build();
            _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            

            // Determine the thread to use.
            Guid threadId;
            ChatHistory? chatHistory = null;
            bool isNewThread = false;
            if (req.ThreadId.HasValue)
            {
                threadId = req.ThreadId.Value;
            }
            else
            {
                threadId = Guid.NewGuid();
            }

            // If no chat history exists, create a new thread.
            if (! await _chatHistoryManager.ThreadExists(threadId))
            {
                var createThreadRequest = new CreateChatThreadRequest { AgentId = agent.Id };
                var threadDto = await _chatHistoryManager.CreateThreadAsync(createThreadRequest);
                threadId = threadDto.Id;
                isNewThread = true;
            }

            var systemMessage = new ChatMessageContent(AuthorRole.System, agent.Instruction);
            await _chatHistoryManager.AddSystemMessageAsync(threadId, systemMessage);

            // Add the user's message.
            await _chatHistoryManager.AddMessageAsync(threadId, req.Message);

            // Execute the chat completion.
            var reducedMessages = await _chatHistoryManager.GetReducedChatHistoryAsync(threadId,_chatCompletionService);
            var executionSettings = new OpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
            var result = await _chatCompletionService.GetChatMessageContentAsync(reducedMessages, executionSettings: executionSettings, kernel: kernel);

            // Create and persist the response message.
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

            // Send the final response.
            await SendAsync(new ChatResponse { Response = responseMessage, ThreadId = threadId }, cancellation: ct);
        }

        private ChatRequest CreateSampleChatRequest()
        {
            var messageContent = new ChatMessageContent(AuthorRole.User, "What is your name?");

            return new ChatRequest
            {
                AgentId = Guid.NewGuid(),
                ThreadId = Guid.NewGuid(),
                Message = messageContent
            };
        }
    }
}
