using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Semantic.Kernel.Agent2AgentProtocol.Example.Agent2;
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
        ? new AzureServiceBusTransport(options.ConnectionString!, options.QueueOrPipeName, isReceiver: false,
            sp.GetRequiredService<ILogger<AzureServiceBusTransport>>())
        : new NamedPipeTransport(options.QueueOrPipeName, isServer: false,
        sp.GetRequiredService<ILogger<NamedPipeTransport>>());
});

services.AddSingleton<Kernel>(_ => Kernel.CreateBuilder().Build());
services.AddSingleton<Agent2>();

ServiceProvider provider = services.BuildServiceProvider();
Agent2 agent = provider.GetRequiredService<Agent2>();
await agent.RunAsync(CancellationToken.None);
