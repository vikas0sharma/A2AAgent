using A2A;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace A2AAgent.Services;

public sealed class NewsAgentHandler : IAgentHandler
{

    private readonly AIAgent _agent;
    private readonly ILogger<NewsAgentHandler> _logger;

    public NewsAgentHandler(OllamaApiClient chatClient, NewsPlugin newsPlugin, ILogger<NewsAgentHandler> logger)
    {
        _agent = chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                Tools = [.. newsPlugin.AsAITools()],
                ToolMode = ChatToolMode.Auto,
                Instructions = "You are a helpful assistant that provides the latest news headlines. Use the provided tools to get the information needed to answer the user's query. Always try to use the tools when relevant information is needed.",
            }
        });
        _logger = logger;
    }

    public async Task ExecuteAsync(RequestContext context, AgentEventQueue eventQueue, CancellationToken cancellationToken)
    {
        var responder = new MessageResponder(eventQueue, context.ContextId);
        string replyText = null;

        var userText = context.UserText;
        if (string.IsNullOrWhiteSpace(userText))
        {
            await responder.ReplyAsync("Please provide a message.", cancellationToken);
            return;
        }

        _logger.LogInformation("Processing message: {UserText}", userText);

        var response = await _agent.RunAsync(userText, cancellationToken: cancellationToken);
        replyText = response.Text;



        if (string.IsNullOrEmpty(replyText))
        {
            replyText = "I couldn't generate a response. Please try again.";
        }

        await responder.ReplyAsync(replyText, cancellationToken);
    }
}
