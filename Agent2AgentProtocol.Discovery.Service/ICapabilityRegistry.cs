namespace Agent2AgentProtocol.Discovery.Service;

public interface ICapabilityRegistry
{
    void RegisterCapability(AgentCapability capability, AgentEndpoint endpoint);
    AgentEndpoint? ResolveCapability(string capabilityName);
}
