namespace Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging
{
    /// <summary>
    /// Abstraction of a duplex transport capable of sending and receiving Agent‑to‑Agent (A2A) protocol JSON‑RPC messages
    /// over a specific medium (e.g., named pipes, Azure Service Bus, WebSockets).
    /// The transport remains message‑shape agnostic: it only deals with raw JSON payloads.
    /// </summary>
    public interface IMessagingTransport
    {
        /// <summary>
        /// Starts listening for incoming A2A messages and invokes the supplied delegate for each
        /// fully received JSON payload.
        /// </summary>
        Task StartProcessingAsync(Func<string, Task> onMessageReceived);

        /// <summary>
        /// Sends the specified raw JSON payload through the underlying transport.
        /// </summary>
        Task SendMessageAsync(string json);

        /// <summary>
        /// Stops the transport listener and releases any underlying resources.
        /// </summary>
        Task StopProcessingAsync();
    }
}
