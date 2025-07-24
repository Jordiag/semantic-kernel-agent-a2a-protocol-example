namespace Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;

/// <summary>
/// Configuration options for selecting and configuring the messaging transport.
/// </summary>
public class TransportOptions
{
    /// <summary>
    /// Use Azure Service Bus instead of named pipes.
    /// </summary>
    public bool UseAzure { get; set; }

    /// <summary>
    /// Connection string used when <see cref="UseAzure"/> is <c>true</c>.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Queue name for Azure Service Bus or pipe name for named pipes.
    /// </summary>
    public string QueueOrPipeName { get; set; } = string.Empty;
}
