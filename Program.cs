using A2A.Server.AspNetCore;
using A2AAgent;

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

app.MapA2AWellKnownAgentEndpoint();

// Protect all A2A endpoints under /a2a/*
var a2aGroup = app.MapGroup("/a2a")
    .RequireAuthorization("A2A");

a2aGroup.MapA2AHttpEndpoint("");

app.MapControllers();

app.Run();
