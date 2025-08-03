using System.Collections.Concurrent;

namespace Agent2AgentProtocol.Discovery.Service;

public class InMemoryCapabilityRegistry : ICapabilityRegistry
{
    private readonly ConcurrentDictionary<string, AgentEndpoint> _capabilities = new();

    public void RegisterCapability(AgentCapability capability, AgentEndpoint endpoint)
    {
        _capabilities[capability.Name] = endpoint;
    }

    public AgentEndpoint? ResolveCapability(string capabilityName)
    {
        return _capabilities.TryGetValue(capabilityName, out var endpoint) ? endpoint : null;
    }
}
