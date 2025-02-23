using FastEndpoints;
using FastEndpoints.Swagger;
using StackExchange.Redis;
using ap.nexus.agents.infrastructure;
using ap.nexus.agents.application;
using AP.Nexus.Core.Extensions;
using ap.nexus.settingmanager.Application;

var builder = WebApplication.CreateBuilder(args);

// First register all infrastructure modules
builder.Services.AddNexusModule<SettingManagerInfrastructureModule>(builder.Configuration);
builder.Services.AddNexusModule<AgentsInfrastructureModule>(builder.Configuration);

// Then register application modules
builder.Services.AddNexusModule<SettingManagerApplicationModule>(builder.Configuration);
builder.Services.AddNexusModule<AgentsApplicationModule>(builder.Configuration);

// Add FastEndpoints.
builder.Services.AddFastEndpoints()
    .SwaggerDocument();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return ConnectionMultiplexer.Connect(configuration.GetValue<string>("Redis:ConnectionString"));
});

var app = builder.Build();


await app.Services.InitializeNexusModulesAsync();

// Configure middleware.
app.UseFastEndpoints()
    .UseSwaggerGen();


app.MapGet("/", () => "Hello World!");

app.Run();
