using Semantic.Kernel.Agent2AgentProtocol.Example.Core.A2A;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Agent1;

public class Agent1(IMessagingTransport transport)
{
    private readonly IMessagingTransport _transport = transport;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("[Agent‑1] starting and listening...");

        // Handle capability cards and task responses
        await _transport.StartProcessingAsync(async json =>
        {
            var (capabilities, from) = A2AHelper.ParseCapabilityCard(json);
            if (capabilities != null)
            {
                Console.WriteLine($"[Agent‑1] capabilities from {from}: {string.Join(", ", capabilities)}");
                return;
            }

            (string? text, _, _) = A2AHelper.ParseTaskRequest(json);
            if (text != null)
            {
                Console.WriteLine($"[Agent‑1] ← {text}");
            }

            await Task.CompletedTask;
        }, cancellationToken);

        await Task.Delay(2_000, cancellationToken); // ensure Agent‑2 listener is ready

        Console.WriteLine("[Agent‑1] → sending REVERSE task...");
        string jsonRequest = A2AHelper.BuildTaskRequest("reverse: hello from Agent 1", "Agent1", "Agent2");
        await _transport.SendMessageAsync(jsonRequest);

        Console.ReadLine();
        await _transport.StopProcessingAsync();
    }
}
