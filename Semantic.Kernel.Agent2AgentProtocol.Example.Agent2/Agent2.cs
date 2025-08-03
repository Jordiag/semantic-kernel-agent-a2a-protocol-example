using Agent2AgentProtocol.Discovery.Service;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.A2A;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.SemanticKernel;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Agent2;

public class Agent2(IMessagingTransport transport, Microsoft.SemanticKernel.Kernel kernel, IOptions<TransportOptions> options)
{
    private readonly IMessagingTransport _transport = transport;
    private readonly TransportOptions _options = options.Value;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("[Agent‑2] waiting for task...");

        await _transport.StartProcessingAsync(async json =>
        {
            (string? text, string? from, string? to) = A2AHelper.ParseTaskRequest(json);
            if (text == null) return;  // not a task message
            if (to != "Agent2")
            {
                Console.WriteLine($"[Agent‑2] ignored message for {to}");
                return;
            }

            Console.WriteLine($"[Agent‑2] received: '{text}' from {from}");

            string result;

            if (text.StartsWith("reverse:", StringComparison.OrdinalIgnoreCase))
            {
                string input = text["reverse:".Length..].Trim();

                Microsoft.SemanticKernel.KernelFunction func = TextProcessingFunction.GetFunctionByType("reverse");
                result = (await kernel.InvokeAsync(func, new() { ["input"] = input })).ToString();
            }
            else
            {
                result = text.StartsWith("upper:", StringComparison.OrdinalIgnoreCase)
                    ? text["upper:".Length..].Trim().ToUpperInvariant()
                    : $"[unhandled] {text}";
            }

            Console.WriteLine($"[Agent‑2] → responding with '{result}'");

            string responseJson = A2AHelper.BuildTaskRequest(result, "Agent2", from ?? string.Empty);
            await _transport.SendMessageAsync(responseJson);
        }, cancellationToken);

        // Register capabilities with discovery service
        using var client = new HttpClient();
        var capability = new AgentCapability { Name = "reverse", AgentId = "Agent2" };
        string transportType = _options.UseAzure ? "ServiceBus" : "NamedPipe";
        var endpoint = new AgentEndpoint { TransportType = transportType, Address = _options.QueueOrPipeName };
        await client.PostAsJsonAsync("http://localhost:5000/register", new { capability, endpoint }, cancellationToken);

        Console.ReadLine();
        await _transport.StopProcessingAsync();
    }
}
