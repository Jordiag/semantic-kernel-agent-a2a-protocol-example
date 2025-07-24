using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Semantic.Kernel.Agent2AgentProtocol.Example.Agent2;
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
        new AzureServiceBusTransport(connStr, "a2a-demo-queue", isReceiver: false,
            sp.GetRequiredService<ILogger<AzureServiceBusTransport>>())); // Agent2 is sender
}
else
{
    services.AddSingleton<IMessagingTransport>(sp =>
        new NamedPipeTransport("a2a-demo-pipe", isServer: false,
            sp.GetRequiredService<ILogger<NamedPipeTransport>>())); // Agent2 is client
}

services.AddSingleton<Kernel>(_ => Kernel.CreateBuilder().Build());
services.AddSingleton<Agent2>();

var provider = services.BuildServiceProvider();
var agent = provider.GetRequiredService<Agent2>();
await agent.RunAsync(CancellationToken.None);
