using Semantic.Kernel.Agent2AgentProtocol.Example.Core.A2A;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.SemanticKernel;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Agent2;

public class Agent2(IMessagingTransport transport, Microsoft.SemanticKernel.Kernel kernel)
{
    private readonly IMessagingTransport _transport = transport;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("[Agent‑2] waiting for task...");

        await _transport.StartProcessingAsync(async json =>
        {
            var (text, from, to) = A2AHelper.ParseTaskRequest(json);
            if (text == null) return;  // not an A2A task message
            if (to != "Agent2")
            {
                Console.WriteLine($"[Agent‑2] ignored message for {to}");
                return;
            }

            Console.WriteLine($"[Agent‑2] received: '{text}' from {from}");

            string result;

            if (text.StartsWith("reverse:", StringComparison.OrdinalIgnoreCase))
            {
                var input = text["reverse:".Length..].Trim();

                var func = TextProcessingFunction.GetFunctionByType("reverse");
                result = (await kernel.InvokeAsync(func, new() { ["input"] = input })).ToString();
            }
            else if (text.StartsWith("upper:", StringComparison.OrdinalIgnoreCase))
            {
                result = text["upper:".Length..].Trim().ToUpperInvariant();
            }
            else
            {
                result = $"[unhandled] {text}";
            }

            Console.WriteLine($"[Agent‑2] → responding with '{result}'");

            var responseJson = A2AHelper.BuildTaskRequest(result, "Agent2", from ?? string.Empty);
            await _transport.SendMessageAsync(responseJson);
        }, cancellationToken);

        Console.ReadLine();
        await _transport.StopProcessingAsync();
    }
}
