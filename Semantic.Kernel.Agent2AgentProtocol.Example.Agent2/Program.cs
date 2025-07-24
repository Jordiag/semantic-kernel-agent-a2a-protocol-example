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
    cfg.UseAzure = true;
    cfg.ConnectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTIONSTRING")!;
    cfg.QueueOrPipeName = "a2a-demo";
});

services.AddSingleton<IMessagingTransport>(sp =>
{
    var options = sp.GetRequiredService<IOptions<TransportOptions>>().Value;
    if (options.UseAzure)
    {
        return new AzureServiceBusTransport(options.ConnectionString!, options.QueueOrPipeName, isReceiver: false,
            sp.GetRequiredService<ILogger<AzureServiceBusTransport>>());
    }

    return new NamedPipeTransport(options.QueueOrPipeName, isServer: false,
        sp.GetRequiredService<ILogger<NamedPipeTransport>>());
});

services.AddSingleton<Kernel>(_ => Kernel.CreateBuilder().Build());
services.AddSingleton<Agent2>();

var provider = services.BuildServiceProvider();
var agent = provider.GetRequiredService<Agent2>();
await agent.RunAsync(CancellationToken.None);
