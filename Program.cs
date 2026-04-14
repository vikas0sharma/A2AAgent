using A2A;
using A2A.AspNetCore;
using A2AAgent;
using A2AAgent.Services;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.ConfigureNewsApiService(builder.Configuration);

// Add Security
var security = StartupExtensions.ConfigureAuthentication(builder.Services, builder.Configuration);
builder.Services.AddAuthorizationBuilder().AddPolicy("A2A", policy => policy.RequireAuthenticatedUser());

// Register OllamaApiClient for DI
builder.Services.AddSingleton<OllamaApiClient>(sp =>
{
    HttpClient httpClient = new()
    {
        BaseAddress = new Uri(builder.Configuration["Ollama:BaseUrl"]!)
    };
    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + (Environment.GetEnvironmentVariable("OLLAMA_APIKEY") ?? builder.Configuration["Ollama:ApiKey"]));
    return new OllamaApiClient(httpClient, builder.Configuration["Ollama:ModelName"]!);
});

var a2aUrl = builder.Configuration["A2AUrl"] ?? "https://localhost:5001/";

// Register A2A agent with the new v1.0 pattern
builder.Services.AddA2AAgent<NewsAgentHandler>(
    new AgentCard
    {
        Name = "NewsAgent",
        Description = "Gets the current worldwide news",
        Version = "1.0.0",
        SupportedInterfaces =
        [
            new AgentInterface
            {
                Url = $"{a2aUrl}a2a",
                ProtocolBinding = "JSONRPC",
                ProtocolVersion = "1.0",
            }
        ],
        Capabilities = new AgentCapabilities
        {
            Streaming = false,
            PushNotifications = false,
        },
        DefaultInputModes = ["text/plain"],
        DefaultOutputModes = ["text/plain"],
        Provider = new AgentProvider { Organization = "Vikas Sharma", Url = "https://github.com/vikas0sharma" },
        Skills =
        [
            new AgentSkill
            {
                Id = "get_top_headlines",
                Name = "get_top_headlines",
                Description = "Gets live top and breaking headlines for a country, specific category in a country",
                Tags = ["news", "headlines"],
            }
        ],
        SecuritySchemes = new Dictionary<string, SecurityScheme> { { security.Key, security.Value } },
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Add this before UseAuthorization
app.UseAuthorization();


// Map A2A endpoints using DI-registered services
var a2aGroup = app.MapGroup("/a2a");

a2aGroup.MapA2A("/").RequireAuthorization("A2A");

// Map well-known agent card at root for spec-compliant discovery
var agentCard = app.Services.GetRequiredService<AgentCard>();
app.MapWellKnownAgentCard(agentCard);

app.MapControllers();

app.Run();
