using A2A.AspNetCore;
using A2AAgent;
using A2AAgent.Services;
using Microsoft.Extensions.AI;
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

var app = builder.Build();

HttpClient httpClient = new()
{
    BaseAddress = new Uri(builder.Configuration["Ollama:BaseUrl"]!)
};
httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + Environment.GetEnvironmentVariable("OLLAMA_APIKEY") ?? builder.Configuration["Ollama:ApiKey"]);

using OllamaApiClient chatClient = new(httpClient, builder.Configuration["Ollama:ModelName"]!);

Microsoft.Agents.AI.ChatClientAgent agent = chatClient.CreateAIAgent(options: new Microsoft.Agents.AI.ChatClientAgentOptions
{
    Name = "NewsAgent",
    Description = "Gets the current worldwide news",
    ChatOptions = new ChatOptions
    {
        Tools = [.. app.Services.GetService<NewsPlugin>()!.AsAITools()],
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Add this before UseAuthorization
app.UseAuthorization();


// Protect all A2A endpoints under /a2a/*
var a2aGroup = app.MapGroup("/a2a")
    .RequireAuthorization("A2A");

// Map A2A endpoints to the protected group instead of the main app
var a2aTaskManager = a2aGroup.MapA2A(
    agent,
    path: "/",  // Empty path since we're already in the /a2a group
    agentCard: new A2A.AgentCard
    {
        Name = "NewsAgent",
        Description = "Gets the current worldwide news",
        Version = "1.0.0",
        Provider = new A2A.AgentProvider { Organization = "Vikas Sharma", Url = "https://github.com/vikas0sharma" },
        Skills = [ new A2A.AgentSkill {
             Id = "get_top_headlines",
             Name = "get_top_headlines",
             Description = "Gets live top and breaking headlines for a country, specific category in a country"
         } ],
        Url = $"{app.Configuration["A2AUrl"]}a2a",
        SecuritySchemes = new Dictionary<string, A2A.SecurityScheme> { { security.Key, security.Value } }
    },
    taskManager => app.MapWellKnownAgentCard(taskManager, "/"));

app.MapControllers();

app.Run();
