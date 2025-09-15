using A2A.Models;
using A2A.Server.Infrastructure;
using A2A.Server.Infrastructure.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using TasksTask = System.Threading.Tasks.Task;

namespace A2AAgent
{
    public class AgentRuntime(Kernel _kernel, ILogger<AgentRuntime> _logger) : IAgentRuntime
    {
        private ConcurrentDictionary<string, CancellationTokenSource> Tasks { get; } = [];
        private ConcurrentDictionary<string, ChatHistory> Sessions { get; } = [];
        private readonly IChatCompletionService _chatService = _kernel.GetRequiredService<IChatCompletionService>();

        public TasksTask CancelAsync(string taskId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cancellation requested for task: {TaskId}", taskId);

            if (Tasks.TryRemove(taskId, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                _logger.LogInformation("Task {TaskId} cancelled successfully", taskId);
            }
            else
            {
                _logger.LogWarning("Task {TaskId} not found for cancellation", taskId);
            }

            return TasksTask.CompletedTask;
        }

        public async IAsyncEnumerable<AgentResponseContent> ExecuteAsync(TaskRecord task, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var taskId = task.Id ?? Guid.NewGuid().ToString();
            var contextId = task.ContextId ?? Guid.NewGuid().ToString();

            _logger.LogInformation("Executing task {TaskId} for context {ContextId}", taskId, contextId);

            // Create a combined cancellation token
            using var taskCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Tasks.TryAdd(taskId, taskCancellationSource);

            // Process the task and yield results
            await foreach (var response in ProcessTaskAsync(task, taskId, contextId, taskCancellationSource))
            {
                yield return response;
            }

            // Clean up
            Tasks.TryRemove(taskId, out _);
            _logger.LogDebug("Task {TaskId} cleanup completed", taskId);
        }

        private async IAsyncEnumerable<AgentResponseContent> ProcessTaskAsync(
            TaskRecord task, 
            string taskId, 
            string contextId, 
            CancellationTokenSource taskCancellationSource)
        {
            AgentResponseContent? result = null;

            try
            {
                // Get or create chat history for this context
                var chatHistory = GetOrCreateSession(contextId);

                // Extract the user message from the task
                var userMessage = ExtractUserMessage(task);
                if (string.IsNullOrWhiteSpace(userMessage))
                {
                    _logger.LogWarning("No valid user message found in task {TaskId}", taskId);
                    result = CreateErrorResponse(task, "No valid message content found");
                }
                else
                {
                    _logger.LogDebug("Processing message: {Message}", userMessage);

                    // Add user message to chat history
                    chatHistory.AddUserMessage(userMessage);

                    _logger.LogInformation($"User: {userMessage}");
                    // Configure execution settings for Gemini
                    var settings = new GeminiPromptExecutionSettings
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                        ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions,
                        MaxTokens = 1000,
                        Temperature = 0.7
                    };

                    // Get response from chat service
                    var response = await _chatService.GetChatMessageContentAsync(
                        chatHistory, settings, _kernel, taskCancellationSource.Token);

                    if (!string.IsNullOrWhiteSpace(response.Content))
                    {
                        // Add response to chat history
                        chatHistory.AddAssistantMessage(response.Content);

                        result = new MessageResponseContent(new Message
                        {
                            MessageId = task.Message?.MessageId ?? Guid.NewGuid().ToString(),
                            ContextId = task.ContextId,
                            Parts = [new TextPart(response.Content)]
                        });

                        _logger.LogDebug("Completed response for task {TaskId}, length: {Length}",
                            taskId, response.Content.Length);
                    }
                    else
                    {
                        _logger.LogWarning("Empty response received for task {TaskId}", taskId);
                        result = CreateErrorResponse(task, "No response generated");
                    }
                }
            }
            catch (OperationCanceledException) when (taskCancellationSource.Token.IsCancellationRequested)
            {
                _logger.LogInformation("Task {TaskId} was cancelled", taskId);
                result = CreateCancellationResponse(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing task {TaskId}: {Error}", taskId, ex.Message);
                result = CreateErrorResponse(task, $"An error occurred: {ex.Message}");
            }

            if (result != null)
            {
                yield return result;
            }
        }

        private ChatHistory GetOrCreateSession(string contextId)
        {
            return Sessions.GetOrAdd(contextId, _ =>
            {
                _logger.LogDebug("Creating new chat session for context {ContextId}", contextId);
                var chatHistory = new ChatHistory();

                // Add system message to set the agent's context
                chatHistory.AddSystemMessage(
                    "You are a helpful news agent that provides current worldwide news information. " +
                    "You have access to news APIs and can retrieve live headlines, breaking news, and news from specific categories or countries. " +
                    "Always provide accurate, timely, and relevant news information when requested.");

                return chatHistory;
            });
        }

        private static string ExtractUserMessage(TaskRecord task)
        {
            if (task.Message?.Parts == null || task.Message.Parts.Count == 0)
                return string.Empty;

            var messageBuilder = new StringBuilder();
            foreach (var part in task.Message.Parts)
            {
                if (part is TextPart textPart && !string.IsNullOrWhiteSpace(textPart.Text))
                {
                    messageBuilder.AppendLine(textPart.Text);
                }
            }

            return messageBuilder.ToString().Trim();
        }

        private static MessageResponseContent CreateErrorResponse(TaskRecord task, string errorMessage)
        {
            return new MessageResponseContent(new Message
            {
                MessageId = task.Message?.MessageId ?? Guid.NewGuid().ToString(),
                ContextId = task.ContextId,
                Parts = [new TextPart($"Error: {errorMessage}")]
            });
        }

        private static MessageResponseContent CreateCancellationResponse(TaskRecord task)
        {
            return new MessageResponseContent(new Message
            {
                MessageId = task.Message?.MessageId ?? Guid.NewGuid().ToString(),
                ContextId = task.ContextId,
                Parts = [new TextPart("The request was cancelled.")]
            });
        }

        // Clean up old sessions periodically (optional enhancement)
        public void CleanupOldSessions(TimeSpan maxAge)
        {
            var cutoffTime = DateTime.UtcNow - maxAge;
            var keysToRemove = new List<string>();

            foreach (var kvp in Sessions)
            {
                // This is a simplified approach - you might want to track session creation times
                if (kvp.Value.Count == 1) // Only system message, likely unused
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                Sessions.TryRemove(key, out _);
                _logger.LogDebug("Cleaned up unused session {ContextId}", key);
            }
        }
    }
}
