using Agent2AgentProtocol.Discovery.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Semantic.Kernel.Agent2AgentProtocol.Example.Agent1;
using Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;
using System.Net.Http.Json;

var services = new ServiceCollection();
services.AddLogging(b => b.AddConsole());

// Discover endpoint for the reverse capability
AgentEndpoint? endpoint;
using(var client = new HttpClient())
{
    endpoint = null;
    Exception? lastException = null;

    for(int attempt = 1; attempt <= 20; attempt++)
    {
        try
        {
            endpoint = await client.GetFromJsonAsync<AgentEndpoint>("http://localhost:5000/resolve/reverse");

            if(endpoint != null)
                break; // Success
        }
        catch(Exception ex)
        {
            lastException = ex;
            await Task.Delay(500);
        }
    }

    if(endpoint == null)
    {
        throw lastException ?? new Exception("Failed to resolve endpoint after 20 attempts.");
    }
}

if (endpoint == null)
{
    Console.WriteLine("Capability 'reverse' not found in registry.");
    return;
}

services.AddSingleton<IMessagingTransport>(sp =>
{
    return endpoint.TransportType == "ServiceBus"
        ? new AzureServiceBusTransport(Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTIONSTRING")!, endpoint.Address, isReceiver: false,
            sp.GetRequiredService<ILogger<AzureServiceBusTransport>>())
        : new NamedPipeTransport(endpoint.Address, isServer: false,
            sp.GetRequiredService<ILogger<NamedPipeTransport>>());
});

services.AddSingleton<Agent1>();

ServiceProvider provider = services.BuildServiceProvider();
Agent1 agent = provider.GetRequiredService<Agent1>();
await agent.RunAsync(CancellationToken.None);
