using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Semantic.Kernel.Agent2AgentProtocol.Example.Agent1;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;

var services = new ServiceCollection();
services.AddLogging(b => b.AddConsole());

// Configure transport
var useAzure = false;

if (useAzure)
{
    var connStr = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTIONSTRING")
                  ?? throw new InvalidOperationException("Set SERVICEBUS_CONNECTIONSTRING");

    services.AddSingleton<IMessagingTransport>(sp =>
        new AzureServiceBusTransport(connStr, "a2a-demo-queue", isReceiver: true,
            sp.GetRequiredService<ILogger<AzureServiceBusTransport>>())); // Agent1 is receiver
}
else
{
    services.AddSingleton<IMessagingTransport>(sp =>
        new NamedPipeTransport("a2a-demo-pipe", isServer: true,
            sp.GetRequiredService<ILogger<NamedPipeTransport>>())); // Agent1 is server
}

// Core dependencies
services.AddSingleton<Agent1>();

// Run
var provider = services.BuildServiceProvider();
var agent = provider.GetRequiredService<Agent1>();
await agent.RunAsync(CancellationToken.None);
