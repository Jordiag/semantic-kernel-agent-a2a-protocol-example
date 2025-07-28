# Semantic Kernel A2A Protocol Example

This repository demonstrates a simple agent-to-agent conversation using the [a2a-dotnet](https://github.com/a2aproject/a2a-dotnet) SDK.

Two console applications (`Agent1` and `Agent2`) exchange A2A JSON-RPC messages over either a named pipe or an Azure Service Bus queue. `Agent2` performs very simple text processing using [Semantic Kernel](https://github.com/microsoft/semantic-kernel) and replies to the sender.

## Prerequisites

- .NET 8.0 SDK
- Optional: Azure Service Bus connection string if you want to use Service Bus instead of named pipes.

## Running the demo

1. Build the solution:
   ```bash
   dotnet build
   ```
2. Start Agent2 (the responder):
   ```bash
   dotnet run --project Semantic.Kernel.Agent2AgentProtocol.Example.Agent2
   ```
3. Start Agent1 in a second terminal:
   ```bash
   dotnet run --project Semantic.Kernel.Agent2AgentProtocol.Example.Agent1
   ```

You should see the agents exchange a `reverse` command. Agent2 reverses the supplied text and streams the response back to Agent1 using the A2A protocol.

## New features

This version uses the `a2a-dotnet` library which provides:

- **A2AClient** for sending JSON‑RPC requests and receiving streaming responses.
- **TaskManager** for managing long running tasks and updating their status.
- Strongly typed models such as `Message` and `TextPart` for constructing requests and parsing replies.

Both streaming and task management are supported by the library and can be enabled by adjusting the helper and transport implementations.
