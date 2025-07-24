using A2A.Models;
using System.Text.Json.Nodes;
using System.Text.Json;
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
        string id = Guid.NewGuid().ToString("N");

        // Build the "message" object using A2A model types so that downstream consumers can
        // deserialize with the official library if they wish.
        var message = new
        {
            role = MessageRole.User.ToString().ToLowerInvariant(),
            parts = new object[]
            {
                new
                {
                    type = "text",
                    text
                }
            }
        };

        var payload = new
        {
            jsonrpc = JsonRpcVersion,
            id,
            method = MethodName,
            @params = new
            {
                message,
                metadata = new
                {
                    from,
                    to
                }
            }
        };

        return JsonSerializer.Serialize(payload);
    }

    /// <summary>
    /// Extracts the user‑supplied text and routing metadata from an A2A JSON‑RPC payload.
    /// Returns <c>(null, null, null)</c> if the payload doesn't match the expected shape.
    /// </summary>
    public static (string? text, string? from, string? to) ParseTaskRequest(string json)
    {
        try
        {
            JsonObject? doc = JsonNode.Parse(json)?.AsObject();
            if (doc?["method"]?.GetValue<string>() != MethodName) return (null, null, null);
            JsonObject? @params = doc?["params"]?.AsObject();
            JsonObject? msg = @params?["message"]?.AsObject();
            string? textPart = msg?["parts"]?[0]?["text"]?.GetValue<string>();

            JsonObject? meta = @params?["metadata"]?.AsObject();
            string? from = meta?["from"]?.GetValue<string>();
            string? to = meta?["to"]?.GetValue<string>();

            return (textPart, from, to);
        }
        catch
        {
            return (null, null, null);
        }
    }
}
