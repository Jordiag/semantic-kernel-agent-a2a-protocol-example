using System.Text.Json;
using System.Text.Json.Nodes;
using A2A;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Core.A2A;

/// <summary>
/// Utility helpers for constructing and parsing A2A protocol JSON‑RPC payloads.
/// We purposely keep the dependency surface minimal and rely on the A2A model types
/// (e.g. <see cref="TextPart"/>, <see cref="MessageRole"/>) for schema correctness.
/// </summary>
public static class A2AHelper
{
    private const string JsonRpcVersion = "2.0";
    private const string MethodName = "agent.task";

    /// <summary>
    /// Creates a JSON‑RPC request message that complies with the A2A specification.
    /// </summary>
    public static string BuildTaskRequest(string text, string from, string to)
    {
        // Build message using the official A2A models
        var msg = new Message
        {
            Role = MessageRole.User,
            MessageId = Guid.NewGuid().ToString(),
            Parts = [new TextPart { Text = text }]
        };

        var sendParams = new MessageSendParams
        {
            Message = msg,
            Metadata = new Dictionary<string, JsonElement>
            {
                ["from"] = JsonSerializer.SerializeToElement(from),
                ["to"] = JsonSerializer.SerializeToElement(to)
            }
        };

        var request = new JsonRpcRequest
        {
            Id = Guid.NewGuid().ToString("N"),
            Method = MethodName,
            Params = JsonSerializer.SerializeToElement(sendParams, A2AJsonUtilities.JsonContext.Default.MessageSendParams)
        };

        return JsonSerializer.Serialize(request, A2AJsonUtilities.JsonContext.Default.JsonRpcRequest);
    }

    /// <summary>
    /// Extracts the user‑supplied text and routing metadata from an A2A JSON‑RPC payload.
    /// Returns <c>(null, null, null)</c> if the payload doesn't match the expected shape.
    /// </summary>
    public static (string? text, string? from, string? to) ParseTaskRequest(string json)
    {
        try
        {
            JsonRpcRequest? request = JsonSerializer.Deserialize(json, A2AJsonUtilities.JsonContext.Default.JsonRpcRequest);
            if (request?.Method != MethodName) return (null, null, null);

            MessageSendParams? parameters = request.Params?.Deserialize(A2AJsonUtilities.JsonContext.Default.MessageSendParams);
            if (parameters?.Message == null) return (null, null, null);

            string? textPart = parameters.Message.Parts.OfType<TextPart>().FirstOrDefault()?.Text;

            parameters.Metadata?.TryGetValue("from", out JsonElement fromEl);
            parameters.Metadata?.TryGetValue("to", out JsonElement toEl);
            string? from = fromEl.GetString();
            string? to = toEl.GetString();

            return (textPart, from, to);
        }
        catch
        {
            return (null, null, null);
        }
    }
}
