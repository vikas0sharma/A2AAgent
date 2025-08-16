using A2A.Models;
using A2A.Server;
using A2A.Server.AspNetCore;
using A2A.Server.Infrastructure;
using A2A.Server.Infrastructure.Services;
using A2AAgent;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure Authentication
(SecurityScheme scheme, string schemeName) = builder.Services.ConfigureAuthentication(builder.Configuration);

// Add authorization
builder.Services.AddAuthorizationBuilder().AddPolicy("A2A", policy => policy.RequireAuthenticatedUser());

builder.Services.AddA2AWellKnownAgent((provider, builder) =>
{

    builder
        .WithName("A2A Agent")
        .WithDescription("Gets the current weather")
        .WithVersion("1.0.0.0")
        .WithProvider(provider => provider
            .WithOrganization("Vikas Sharma")
            .WithUrl(new("https://github.com/vikas0sharma")))
        .SupportsStreaming()
        .WithUrl(new("/a2a", UriKind.Relative))
        .WithSkill(skill => skill
            .WithId("get_current_weather_info")
            .WithName("CurrentWeatherInfo")
            .WithDescription("Gets the current weather information"))
        .WithSecurityScheme(schemeName!, scheme!);
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<IAgentRuntime, AgentRuntime>();
builder.Services.AddA2AProtocolServer(builder =>
{
    builder
        .UseAgentRuntime(provider => provider.GetRequiredService<IAgentRuntime>())
        .UseDistributedCacheTaskRepository()
        .SupportsStreaming();
});

builder.Services.ConfigureNewsApi(builder.Configuration);
builder.Services.ConfigureSemanticKernel(builder.Configuration);


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

app.MapA2AWellKnownAgentEndpoint();

// Protect all A2A endpoints under /a2a/*
var a2aGroup = app.MapGroup("/a2a")
    .RequireAuthorization("A2A");

a2aGroup.MapA2AHttpEndpoint("");

app.MapControllers();

app.Run();
