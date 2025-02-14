using FastEndpoints;
using ap.nexus.agents.infrastructure.Extensions;
using FastEndpoints.Swagger;
using ap.nexus.agents.application.Extensions;
using StackExchange.Redis;
using ap.nexus.agents.infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ap.nexus.agents.infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Register infrastructure services.
builder.Services.AddInfrastructureServices(builder.Configuration);

//Register application services.
builder.Services.AddApplicationServices(builder.Configuration);

// Add FastEndpoints.
builder.Services.AddFastEndpoints()
    .SwaggerDocument();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return ConnectionMultiplexer.Connect(configuration.GetValue<string>("Redis:ConnectionString"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope()) 
{ 
    var context = scope.ServiceProvider.GetRequiredService<AgentsDbContext>(); 
    context.Database.Migrate(); DatabaseSeeder.Seed(context); 
}

// Configure middleware.
app.UseFastEndpoints()
    .UseSwaggerGen();


app.MapGet("/", () => "Hello World!");

app.Run();
