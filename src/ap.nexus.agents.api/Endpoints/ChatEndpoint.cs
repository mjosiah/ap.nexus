using ap.nexus.abstractions.Agents.DTOs;
using ap.nexus.abstractions.Agents.Interfaces;
using Azure.Core;
using Azure;
using FastEndpoints;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ap.nexus.agents.api.Endpoints
{
    public class ChatRequest
    {
        [Required]
        public Guid AgentExternalId { get; set; }
        public Guid? ThreadExternalId { get; set; }
        [Required]
        public ChatMessageContent Message { get; set; }
    }

    public class ChatResponse
    {
        public ChatMessageContent Response { get; set; } = new();
        public Guid ThreadExternalId { get; set; }
    }
    public class ChatEndpoint : Endpoint<ChatRequest, ChatResponse>
    {
        private readonly IAgentService _agentService;
        private readonly IThreadService _threadService;
        private readonly IMessageService _messageService;
        private readonly IKernelBuilder _kernelBuilder;
        private IChatCompletionService? _chatCompletionService;
        private readonly IConfiguration _configuration;

        public ChatEndpoint(IAgentService agentService, IThreadService threadService, IMessageService messageService, IConfiguration configuration)
        {
            _agentService = agentService;
            _threadService = threadService;
            _messageService = messageService;
            _configuration = configuration;

        }

        public override void Configure()
        {
            
            var messageContent = new ChatMessageContent(AuthorRole.User, "This is a polymorphic message.");
            var textContent = new TextContent { Text = "Sample text content" };
            var imageContent = new ImageContent { Uri = new Uri("http://example.com/image.jpg") };

            messageContent.Items.Add(textContent);
            messageContent.Items.Add(imageContent);

            var request = new ChatRequest { AgentExternalId = Guid.NewGuid(), ThreadExternalId = Guid.NewGuid(), Message = messageContent};

            Post("/chat");
            AllowAnonymous();
            Summary(s => {
                s.Summary = "Handles chat requests using Semantic Kernel agents.";
                s.Description = "Handles chat requests using Semantic Kernel agents.";
                s.RequestExamples.Add(new FastEndpoints.RequestExample(JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true })));
            });
        }

        public override async Task HandleAsync(ChatRequest req, CancellationToken ct)
        {
            var agent = await _agentService.GetAgentByExternalIdAsync(req.AgentExternalId);
            if (agent == null)
            {
                AddError("Agent not found.");
                ThrowIfAnyErrors();
                return;
            }

            // Get OpenAI configuration from IConfiguration
            var modelId = _configuration["OpenAI:ModelId"];
            var endpoint = _configuration["OpenAI:Endpoint"];
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrEmpty(modelId) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                AddError("OpenAI configuration is missing. Check your configuration.");
                ThrowIfAnyErrors();
                return;
            }
            // Create a kernel with Azure OpenAI chat completion
            var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);
            // Add enterprise components
            builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

            // Build the kernel
            Kernel kernel = builder.Build();
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // **Agent Building:** Construct the system prompt
            string systemPrompt = $@"{agent.Instruction}";

            var history = new ChatHistory();
            history.AddSystemMessage(systemPrompt);

            Guid threadExternalId = req.ThreadExternalId ?? Guid.NewGuid();

            var thread = await _threadService.GetThreadByExternalIdAsync(threadExternalId);
            if (thread == null)
            {
                var createThreadRequest = new CreateChatThreadRequest {AgentExternalId = agent.ExternalId };
                thread = await _threadService.CreateThreadAsync(createThreadRequest);
            }

            var previousMessages = await _messageService.GetMessagesByThreadExternalIdAsync(threadExternalId);

            foreach (var msg in previousMessages)
            {
                history.AddMessage(msg.Role, msg.Content);
            }

            history.AddMessage(req.Message.Role, req.Message.Content);

            // Enable planning
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var result = await _chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            var responseContent = result.Content ?? string.Empty;
            var responseRole = result.Role;

            var responseMessage = new ChatMessageContent { Content = responseContent, Role = responseRole };

            await _messageService.AddMessageAsync(req.Message, threadExternalId);
            await _messageService.AddMessageAsync(responseMessage, threadExternalId);

            await SendAsync(new ChatResponse { Response = responseMessage, ThreadExternalId = threadExternalId }, cancellation: ct);
        }
    }
}
