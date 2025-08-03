namespace Agent2AgentProtocol.Discovery.Service;

public class CapabilityRegistration
{
    public AgentCapability Capability { get; set; } = new();
    public AgentEndpoint Endpoint { get; set; } = new();
}

