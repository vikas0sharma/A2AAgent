using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using System.Threading.Tasks;

namespace A2AAgent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController(ILogger<ChatController> _logger, IChatCompletionService _chatService, Kernel _kernel) : ControllerBase
    {

        [HttpPost]
        public async Task<string> Post([FromBody] string message)
        {
            _logger.LogInformation("POST request received with message: {Message}", message);
            GeminiPromptExecutionSettings settings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Required(),
                ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
            };
            try
            {
                var response = await _chatService.GetChatMessageContentAsync(message, settings, _kernel);
                return response.Content;
            }
            catch (Exception e)
            {
                Exception ex = e;
                throw;
            }
        }
    }
}
