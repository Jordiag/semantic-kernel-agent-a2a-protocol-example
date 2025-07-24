using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Semantic.Kernel.Agent2AgentProtocol.Example.Agent2;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;

var services = new ServiceCollection();

// Configure transport
var useAzure = false;

if (useAzure)
{
    var connStr = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTIONSTRING")
                  ?? throw new InvalidOperationException("Set SERVICEBUS_CONNECTIONSTRING");

    services.AddSingleton<IMessagingTransport>(_ =>
        new AzureServiceBusTransport(connStr, "a2a-demo-queue", isReceiver: false)); // Agent2 is sender
}
else
{
    services.AddSingleton<IMessagingTransport>(_ =>
        new NamedPipeTransport("a2a-demo-pipe", isServer: false)); // Agent2 is client
}

services.AddSingleton<Kernel>(_ => Kernel.CreateBuilder().Build());
services.AddSingleton<Agent2>();

var provider = services.BuildServiceProvider();
var agent = provider.GetRequiredService<Agent2>();
await agent.RunAsync();
