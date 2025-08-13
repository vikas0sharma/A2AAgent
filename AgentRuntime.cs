using A2A.Server.Infrastructure;
using A2A.Server.Infrastructure.Services;
using System.Threading.Tasks;

namespace A2AAgent
{
    public class AgentRuntime : IAgentRuntime
    {
        Task IAgentRuntime.CancelAsync(string taskId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        async IAsyncEnumerable<AgentResponseContent> IAgentRuntime.ExecuteAsync(TaskRecord task, CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken);
            //yield return new ArtifactResponseContent(new A2A.Models.Artifact
            //{
            //    Parts = [new A2A.Models.TextPart("Today its going to be a Happy day with nice temperature 21 degree celsius.")]
            //}, false, true);

            yield return new MessageResponseContent(new A2A.Models.Message
            {
                MessageId = task.Message.MessageId,
                ContextId = task.ContextId,
                Parts = [new A2A.Models.TextPart("Today its going to be a Happy day with nice temperature 21 degree celsius.")]
            });

        }
    }
}
