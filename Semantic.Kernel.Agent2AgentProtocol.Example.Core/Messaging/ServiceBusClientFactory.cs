using Azure.Messaging.ServiceBus;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;

public static class ServiceBusClientFactory
{
    public static ServiceBusClient Create(string connectionString)
        => new(connectionString);
}