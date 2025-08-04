# Semantic Kernel A2A protocol and A2A Discovery Service Example

This repository showcases a playful yet practical conversation between two autonomous agents using the [a2a‑dotnet](https://github.com/a2aproject/a2a-dotnet) SDK and [Semantic Kernel](https://github.com/microsoft/semantic-kernel).  It demonstrates how agents can discover each other, exchange JSON‑RPC messages, and stream results back in real time — all while keeping the code simple and transparent.

> **✨ What makes it cool?**
>
> * Agents locate one another through a lightweight discovery service.
> * Messages hop over either a local named pipe or Azure Service Bus.
> * Semantic Kernel powers text transformations such as reversing or upper‑casing.
> * The sample is small enough to grok in minutes yet flexible enough to extend.

---

## Repository layout

The solution is split into several projects to keep responsibilities clear:

| Folder | Description |
| ------ | ----------- |
| `Agent2AgentProtocol.Discovery.Service` | Minimal web API that registers agent capabilities and resolves them to endpoints. |
| `Semantic.Kernel.Agent2AgentProtocol.Example.Agent1` | Console app that discovers a capability and sends a task. |
| `Semantic.Kernel.Agent2AgentProtocol.Example.Agent2` | Console app that registers capabilities and processes incoming tasks. |
| `Semantic.Kernel.Agent2AgentProtocol.Example.Core` | Shared helpers for A2A messages, transports, and Semantic Kernel functions. |

Each project targets **.NET 8** and uses the latest `a2a-dotnet` packages.

---

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- Optional: An Azure Service Bus connection string if you want to see the agents communicate through the cloud rather than a named pipe.

---

## Running the demo

# Visual Studio, set all prject to run at the same time:
<img width="802" height="401" alt="image" src="https://github.com/user-attachments/assets/3debf096-d132-4c61-864f-ca16c8b79b32" />

# Command line
1. **Build the solution**

   ```bash
   dotnet build
   ```

2. **Start the discovery service**

   Agents query this lightweight API to find where other agents are listening.

   ```bash
   dotnet run --project Agent2AgentProtocol.Discovery.Service
   ```

3. **Start Agent 2 (the responder)**

   Agent 2 registers the `reverse` and `upper` capabilities and waits for work.

   ```bash
   dotnet run --project Semantic.Kernel.Agent2AgentProtocol.Example.Agent2
   ```

4. **Start Agent 1 (the initiator)**

   Agent 1 asks the discovery service for an endpoint that can handle a reverse command and then sends a task message.

   ```bash
   dotnet run --project Semantic.Kernel.Agent2AgentProtocol.Example.Agent1
   ```

5. **Watch the conversation**

   The terminal output shows Agent 2 reversing text and streaming the response back to Agent 1.  Try editing the request in `Agent1.cs` to send `upper:` instead of `reverse:`.

---

## Switching to Azure Service Bus

Want to see the agents chat over the cloud?

1. Create a Service Bus namespace and queue.
2. Set the environment variable `SERVICEBUS_CONNECTIONSTRING` with a connection string that has **Send** and **Listen** rights.
3. In `appsettings.json` for both agents, change `UseAzureServiceBus` to `true` *or* modify the `TransportOptions` in `Agent2` to set `UseAzure = true`.
4. Run the steps above; messages now traverse Service Bus instead of a named pipe.

The transport implementation is abstracted behind `IMessagingTransport`, so you can substitute other mechanisms (web sockets, HTTP, etc.) with minimal changes.

---

## Architecture overview

<img width="1361" height="904" alt="output-onlinepngtools (5)" src="https://github.com/user-attachments/assets/49155240-efc4-4cd6-af87-d0292b07aa99" />

Agent 2 exposes skills via Semantic Kernel functions.  When it receives a task that begins with `reverse:` or `upper:`, it invokes the appropriate function and replies with the result.  The agents can easily be extended with additional skills by updating `TextProcessingFunction.cs` and registering new capabilities.

---

## Extending the sample

- Add new text transformations by creating Semantic Kernel functions and listing them in `BuildCapabilitiesCard`.
- Replace the transports with your preferred messaging system by implementing `IMessagingTransport`.
- Integrate a real discovery mechanism or directory service for more sophisticated routing.
- Use the `TaskManager` provided by `a2a-dotnet` to track long‑running jobs or to stream incremental progress.

---

## Contributing and feedback

Pull requests and issues are welcome!  This repository is intentionally lightweight; ideas for additional skills, transports, or polish are encouraged.

---

## Resources

- [a2a‑dotnet SDK](https://github.com/a2aproject/a2a-dotnet)
- [Semantic Kernel](https://github.com/microsoft/semantic-kernel)
- [Azure Service Bus](https://learn.microsoft.com/azure/service-bus-messaging/service-bus-messaging-overview)

---

## License

Licensed under the [MIT License](LICENSE).

