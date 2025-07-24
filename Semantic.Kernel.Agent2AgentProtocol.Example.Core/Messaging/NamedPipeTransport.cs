using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Core.Messaging;

/// <summary>
/// Implementation of <see cref="IMessagingTransport"/> based on <see cref="NamedPipeServerStream"/> /
/// <see cref="NamedPipeClientStream"/>. The transport deals with **raw A2A JSON‑RPC messages** – every logical
/// message is framed on its own line (\n‑delimited).
/// </summary>
public sealed class NamedPipeTransport(string pipeName, bool isServer) : IMessagingTransport
{
    private readonly string _pipeName = pipeName;
    private readonly bool _isServer = isServer;
    private Stream? _stream;
    private Func<string, Task>? _handler;
    private CancellationTokenSource? _cts;

    public async Task StartProcessingAsync(Func<string, Task> onMessageReceived)
    {
        _handler = onMessageReceived;
        _cts = new CancellationTokenSource();

        if (_isServer)
        {
            var server = new NamedPipeServerStream(
                _pipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            await server.WaitForConnectionAsync(_cts.Token);
            _stream = server;
        }
        else
        {
            var client = new NamedPipeClientStream(
                ".",
                _pipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);

            await client.ConnectAsync(_cts.Token);
            _stream = client;
        }

        _ = Task.Run(ReadLoopAsync, _cts.Token); // fire‑and‑forget
    }

    private async Task ReadLoopAsync()
    {
        if (_stream == null || _handler == null) return;

        var reader = new StreamReader(_stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        while (!_cts?.IsCancellationRequested ?? false)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                // Validate that line is valid JSON so we don't forward garbage.
                JsonDocument.Parse(line);
                await _handler(line);
            }
            catch (JsonException)
            {
                // Skip malformed payloads to avoid breaking the loop.
                continue;
            }
        }
    }

    public async Task SendMessageAsync(string json)
    {
        if (_stream == null) return;

        var writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
        await writer.WriteLineAsync(json);
    }

    public async Task StopProcessingAsync()
    {
        _cts?.Cancel();
        _stream?.Dispose();
        _cts?.Dispose();
        await Task.Yield();
    }
}
