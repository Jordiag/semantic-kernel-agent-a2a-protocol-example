namespace Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;

public interface IMessageProcessor
{
    Task StartProcessingAsync();
    Task StopProcessingAsync();
}
