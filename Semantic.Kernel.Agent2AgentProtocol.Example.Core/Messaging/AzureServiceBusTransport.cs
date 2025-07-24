using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;

/// <summary>
/// A2A transport implemented on top of Azure Service Bus queues.
/// Each message body contains the raw JSON‑RPC payload produced by the A2A library.
/// The queue name is used symmetrically by *both* agents – the <paramref name="isReceiver"/> flag merely
/// decides which side sets up the <see cref="ServiceBusProcessor"/>.
/// </summary>
public sealed class AzureServiceBusTransport(string connectionString, string queueName, bool isReceiver)
    : IMessagingTransport
{
    private readonly ServiceBusClient _client = new(connectionString);
    private readonly string _queueName = queueName;
    private readonly bool _isReceiver = isReceiver;

    private ServiceBusSender? _sender;
    private ServiceBusProcessor? _processor;
    private Func<string, Task>? _handler;

    public async Task StartProcessingAsync(Func<string, Task> onMessageReceived)
    {
        _handler = onMessageReceived;
        _sender = _client.CreateSender(_queueName);

        if (_isReceiver)
        {
            _processor = _client.CreateProcessor(_queueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });

            _processor.ProcessMessageAsync += async args =>
            {
                var json = args.Message.Body.ToString();
                try
                {
                    JsonDocument.Parse(json);
                    if (_handler != null) await _handler(json);
                }
                catch (JsonException)
                {
                    // Ignore malformed payloads
                }

                await args.CompleteMessageAsync(args.Message);
            };

            _processor.ProcessErrorAsync += args =>
            {
                Console.Error.WriteLine(args.Exception);
                return Task.CompletedTask;
            };

            await _processor.StartProcessingAsync();
        }
    }

    public async Task SendMessageAsync(string json)
    {
        _sender ??= _client.CreateSender(_queueName);
        var message = new ServiceBusMessage(json);
        await _sender.SendMessageAsync(message);
    }

    public async Task StopProcessingAsync()
    {
        if (_processor != null) await _processor.StopProcessingAsync();
        await _client.DisposeAsync();
    }
}
