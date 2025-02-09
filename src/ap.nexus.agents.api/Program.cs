using FastEndpoints;
using ap.nexus.agents.infrastructure.Extensions;
using FastEndpoints.Swagger;
using ap.nexus.agents.application.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register infrastructure services.
builder.Services.AddInfrastructureServices(builder.Configuration);

//Register application services.
builder.Services.AddApplicationServices(builder.Configuration);

// Add FastEndpoints.
builder.Services.AddFastEndpoints()
    .SwaggerDocument();


var app = builder.Build();

// Configure middleware.
app.UseFastEndpoints()
    .UseSwaggerGen();


app.MapGet("/", () => "Hello World!");

app.Run();
