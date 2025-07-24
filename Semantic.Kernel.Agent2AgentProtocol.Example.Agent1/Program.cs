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
    cfg.UseAzure = true;
    cfg.ConnectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTIONSTRING")!;
    cfg.QueueOrPipeName = "a2a-demo";
});

services.AddSingleton<IMessagingTransport>(sp =>
{
    var options = sp.GetRequiredService<IOptions<TransportOptions>>().Value;
    if (options.UseAzure)
    {
        return new AzureServiceBusTransport(options.ConnectionString!, options.QueueOrPipeName, isReceiver: true,
            sp.GetRequiredService<ILogger<AzureServiceBusTransport>>());
    }

    return new NamedPipeTransport(options.QueueOrPipeName, isServer: true,
        sp.GetRequiredService<ILogger<NamedPipeTransport>>());
});

// Core dependencies
services.AddSingleton<Agent1>();

// Run
var provider = services.BuildServiceProvider();
var agent = provider.GetRequiredService<Agent1>();
await agent.RunAsync(CancellationToken.None);
