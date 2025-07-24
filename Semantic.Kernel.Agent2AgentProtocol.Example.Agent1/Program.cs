using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Semantic.Kernel.Agent2AgentProtocol.Example.Agent1;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;

var services = new ServiceCollection();
services.AddLogging(b => b.AddConsole());

// Configure transport via options
services.Configure<TransportOptions>(cfg =>
{
    cfg.UseAzure = false;
    cfg.ConnectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTIONSTRING")!;
    cfg.QueueOrPipeName = "a2a-demo";
});

services.AddSingleton<IMessagingTransport>(sp =>
{
    TransportOptions options = sp.GetRequiredService<IOptions<TransportOptions>>().Value;
    return options.UseAzure
        ? new AzureServiceBusTransport(options.ConnectionString!, options.QueueOrPipeName, isReceiver: true,
            sp.GetRequiredService<ILogger<AzureServiceBusTransport>>())
        : new NamedPipeTransport(options.QueueOrPipeName, isServer: true,
        sp.GetRequiredService<ILogger<NamedPipeTransport>>());
});

// Core dependencies
services.AddSingleton<Agent1>();

// Run
ServiceProvider provider = services.BuildServiceProvider();
Agent1 agent = provider.GetRequiredService<Agent1>();
await agent.RunAsync(CancellationToken.None);
