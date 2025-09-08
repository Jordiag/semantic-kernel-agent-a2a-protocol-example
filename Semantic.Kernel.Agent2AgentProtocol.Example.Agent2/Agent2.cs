using Agent2AgentProtocol.Discovery.Service;
using System.Text.Json;
using A2A;
using Microsoft.Extensions.Logging;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.A2A;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.SemanticKernel;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Agent2;

public class Agent2(
    IMessagingTransport transport,
    Microsoft.SemanticKernel.Kernel kernel,
    IOptions<TransportOptions> options,
    ILogger<Agent2> logger)
{
    private readonly IMessagingTransport _transport = transport;
    private readonly TransportOptions _options = options.Value;
    private readonly ILogger<Agent2> _logger = logger;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Agent-2] waiting for task...");

        // Register capabilities with discovery service
        using var client = new HttpClient();
        var capability = new AgentCapability { Name = "reverse", AgentId = "Agent2" };
        string transportType = _options.UseAzure ? "ServiceBus" : "NamedPipe";
        var endpoint = new AgentEndpoint { TransportType = transportType, Address = _options.QueueOrPipeName };
        await client.PostAsJsonAsync("http://localhost:5000/register", new { capability, endpoint }, cancellationToken);
        _logger.LogInformation("[Agent-2] Registered capabilities with discovery service");

        await _transport.StartProcessingAsync(async json =>
        {
            AgentMessage? message = JsonSerializer.Deserialize<AgentMessage>(json, A2AJsonUtilities.DefaultOptions);
            if (message == null) return; // not a valid message

            (string? text, string? from, string? to) = A2AHelper.ParseTaskRequest(message);
            if (text == null) return;  // not a task message
            if (to != "Agent2")
            {
                _logger.LogWarning("[Agent-2] ignored message for {To}", to);
                return;
            }

            _logger.LogInformation("[Agent-2] received: '{Text}' from {From}", text, from);

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

            _logger.LogInformation("[Agent-2] → responding with '{Result}'", result);

            AgentMessage response = A2AHelper.BuildTaskRequest(result, "Agent2", from ?? string.Empty);
            string responseJson = JsonSerializer.Serialize(response, A2AJsonUtilities.DefaultOptions);
            await _transport.SendMessageAsync(responseJson);
        }, cancellationToken);

        Console.ReadLine();
        await _transport.StopProcessingAsync();
    }
}
