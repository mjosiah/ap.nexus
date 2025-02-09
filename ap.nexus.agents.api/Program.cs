using FastEndpoints;
using ap.nexus.agents.infrastructure.Extensions;
using ap.nexus.agents.application.Services;
using ap.nexus.abstractions.Agents.Interfaces;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Register infrastructure services.
builder.Services.AddInfrastructureServices(builder.Configuration);

// Register application services.
builder.Services.AddScoped<IAgentService, AgentService>();

// Add FastEndpoints.
builder.Services.AddFastEndpoints()
    .SwaggerDocument();


var app = builder.Build();

// Configure middleware.
app.UseFastEndpoints()
    .UseSwaggerGen();


app.MapGet("/", () => "Hello World!");

app.Run();
