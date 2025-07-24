using Microsoft.Extensions.DependencyInjection;
using Semantic.Kernel.Agent2AgentProtocol.Example.Agent1;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;

var services = new ServiceCollection();

// Configure transport
var useAzure = false;

if (useAzure)
{
    var connStr = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTIONSTRING")
                  ?? throw new InvalidOperationException("Set SERVICEBUS_CONNECTIONSTRING");

    services.AddSingleton<IMessagingTransport>(_ =>
        new AzureServiceBusTransport(connStr, "a2a-demo-queue", isReceiver: true)); // Agent1 is receiver
}
else
{
    services.AddSingleton<IMessagingTransport>(_ =>
        new NamedPipeTransport("a2a-demo-pipe", isServer: true)); // Agent1 is server
}

// Core dependencies
services.AddSingleton<Agent1>();

// Run
var provider = services.BuildServiceProvider();
var agent = provider.GetRequiredService<Agent1>();
await agent.RunAsync();
