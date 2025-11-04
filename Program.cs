using A2A.Server.AspNetCore;
using A2AAgent;
using Microsoft.Extensions.Options;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure the required services for A2A
builder.Services.ConfigureA2AServerWithAuth(builder.Configuration);
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

//app.MapA2AWellKnownAgentEndpoint();

app.Map("/.well-known/agents.json", a =>
{
    AgentCardExtended card = a.ApplicationServices.GetService<AgentCardExtended>()!;
    app.Use(async (HttpContext context, RequestDelegate next) =>
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        await context.Response.WriteAsJsonAsync(card, context.RequestServices.GetRequiredService<IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>().Value.JsonSerializerOptions, context.RequestAborted);
    });
});

// Protect all A2A endpoints under /a2a/*
var a2aGroup = app.MapGroup("/a2a")
    .RequireAuthorization("A2A");

a2aGroup.MapA2AHttpEndpoint("");

app.MapControllers();

app.Run();
