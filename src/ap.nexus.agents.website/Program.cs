using ap.nexus.agents.apiclient;
using ap.nexus.agents.website;
using ap.nexus.agents.website.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register Refit client for the Chat API
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services
    .AddRefitClient<IChatApiClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl));

// Add client-side services
builder.Services.AddScoped<StateContainer>();
builder.Services.AddScoped<IChatService, ChatApiService>();
builder.Services.AddScoped<UserService>();

await builder.Build().RunAsync();
