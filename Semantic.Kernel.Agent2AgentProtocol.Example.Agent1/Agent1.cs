using Semantic.Kernel.Agent2AgentProtocol.Example.Core.A2A;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Agent1;

public class Agent1(IMessagingTransport transport)
{
    private readonly IMessagingTransport _transport = transport;

    public async Task RunAsync()
    {
        Console.WriteLine("[Agent‑1] starting and listening...");

        // Print any answers we receive
        await _transport.StartProcessingAsync(async json =>
        {
            Console.WriteLine($"[Agent‑1] ← {json}");
            await Task.CompletedTask;
        });

        await Task.Delay(2_000); // ensure Agent‑2 listener is ready

        Console.WriteLine("[Agent‑1] → sending REVERSE task...");
        var jsonRequest = A2AHelper.BuildTaskRequest("reverse: hello from Agent 1", "Agent1", "Agent2");
        await _transport.SendMessageAsync(jsonRequest);

        Console.ReadLine();
        await _transport.StopProcessingAsync();
    }
}
