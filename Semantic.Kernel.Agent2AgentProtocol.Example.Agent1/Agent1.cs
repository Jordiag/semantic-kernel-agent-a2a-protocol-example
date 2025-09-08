using System.Text.Json;
using A2A;
using Microsoft.Extensions.Logging;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.A2A;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Agent1;

public class Agent1(IMessagingTransport transport, ILogger<Agent1> logger)
{
    private readonly IMessagingTransport _transport = transport;
    private readonly ILogger<Agent1> _logger = logger;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Agent-1] starting and listening...");

        // Handle task responses
        await _transport.StartProcessingAsync(async json =>
        {
            AgentMessage? message = JsonSerializer.Deserialize<AgentMessage>(json, A2AJsonUtilities.DefaultOptions);
            if (message != null)
            {
                (string? text, _, _) = A2AHelper.ParseTaskRequest(message);
                if (text != null)
                {
                    _logger.LogInformation("[Agent-1] received response from another agent ← '{Text}'", text);
                }
            }

            await Task.CompletedTask;
        }, cancellationToken);

        await Task.Delay(2000, cancellationToken); // ensure Agent‑2 listener is ready

        _logger.LogInformation("[Agent-1] → sending REVERSE task...");
        AgentMessage request = A2AHelper.BuildTaskRequest("reverse: hello from Agent 1", "Agent1", "Agent2");
        string jsonRequest = JsonSerializer.Serialize(request, A2AJsonUtilities.DefaultOptions);
        await _transport.SendMessageAsync(jsonRequest);

        Console.ReadLine();
        await _transport.StopProcessingAsync();
    }
}
