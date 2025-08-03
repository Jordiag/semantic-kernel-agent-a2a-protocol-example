using Azure.Messaging.ServiceBus;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;

/// <summary>
/// A2A transport implemented on top of Azure Service Bus queues.
/// Each message body contains the raw JSON‑RPC payload produced by the A2A library.
/// The queue name is used symmetrically by *both* agents – the <paramref name="isReceiver"/> flag merely
/// decides which side sets up the <see cref="ServiceBusProcessor"/>.
/// </summary>
public sealed class AzureServiceBusTransport(string connectionString, string queueName, bool isReceiver, ILogger<AzureServiceBusTransport>? logger = null)
    : IMessagingTransport
{
    private readonly ServiceBusClient _client = new(connectionString);
    private readonly string _queueName = queueName;
    private readonly bool _isReceiver = isReceiver;

    private ServiceBusSender? _sender;
    private ServiceBusProcessor? _processor;
    private Func<string, Task>? _handler;
    private readonly ILogger<AzureServiceBusTransport>? _logger = logger;
    private CancellationTokenSource? _cts;

    public async Task StartProcessingAsync(Func<string, Task> onMessageReceived, CancellationToken cancellationToken)
    {
        _handler = onMessageReceived;
        _sender = _client.CreateSender(_queueName);
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        if (_isReceiver)
        {
            _processor = _client.CreateProcessor(_queueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });

            _processor.ProcessMessageAsync += async args =>
            {
                string json = args.Message.Body.ToString();
                try
                {
                    JsonDocument.Parse(json);
                    _logger?.LogDebug("Received JSON: {json}", json);
                    if (_handler != null) await _handler(json);
                }
                catch (JsonException ex)
                {
                    // Ignore malformed payloads
                    _logger?.LogWarning(ex,"Received malformed JSON");
                }

                await args.CompleteMessageAsync(args.Message);
            };

            _processor.ProcessErrorAsync += args =>
            {
                Console.Error.WriteLine(args.Exception);
                return Task.CompletedTask;
            };

            await _processor.StartProcessingAsync(_cts.Token);
        }
    }

    public async Task SendMessageAsync(string json)
    {
        _sender ??= _client.CreateSender(_queueName);
        var message = new ServiceBusMessage(json)
        {
            ContentType = "application/json"
        };
        try
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("id", out JsonElement idProperty))
            {
                message.CorrelationId = idProperty.GetString();
            }
        }
        catch (JsonException ex)
        {
            _logger?.LogWarning(ex, "Malformed JSON when sending");
        }
        await _sender.SendMessageAsync(message);
        _logger?.LogDebug("Sent JSON: {json}", json);
    }

    public async Task StopProcessingAsync()
    {
        if(_cts != null) await _cts.CancelAsync();
        if (_processor != null) await _processor.StopProcessingAsync();
        await _client.DisposeAsync();
        _cts?.Dispose();
    }
}
