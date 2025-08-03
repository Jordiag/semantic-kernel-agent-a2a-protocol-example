using System.Text.Json;
using A2A;
using System.Linq;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Core.A2A;

/// <summary>
/// Helper methods for creating and parsing A2A protocol payloads.
/// </summary>
public static class A2AHelper
{
    private static readonly JsonSerializerOptions s_options = A2AJsonUtilities.DefaultOptions;

    /// <summary>
    /// Build an <see cref="AgentCard"/> describing this agent's capabilities.
    /// </summary>
    public static string BuildCapabilitiesCard(string from, string to)
    {
        var card = new AgentCard
        {
            Name = from,
            Description = "Agent capabilities",
            Url = $"https://{from}.a2a.local/",
            Version = "1.0.0",
            Capabilities = new AgentCapabilities(),
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Skills =
            [
                new AgentSkill { Id = "reverse", Name = "reverse", Description = "Reverse text", Tags = [] },
                new AgentSkill { Id = "upper", Name = "upper", Description = "Uppercase text", Tags = [] }
            ]
        };

        return JsonSerializer.Serialize(card, s_options);
    }

    /// <summary>
    /// Build a <see cref="Message"/> used to send a text task.
    /// </summary>
    public static string BuildTaskRequest(string text, string from, string to)
    {
        var message = new Message
        {
            Role = MessageRole.Agent,
            MessageId = Guid.NewGuid().ToString(),
            Parts = [new TextPart { Text = text }],
            Metadata = new Dictionary<string, JsonElement>
            {
                ["from"] = JsonSerializer.SerializeToElement(from, s_options),
                ["to"] = JsonSerializer.SerializeToElement(to, s_options)
            }
        };

        return JsonSerializer.Serialize(message, s_options);
    }

    /// <summary>
    /// Parse a message looking for a text task request.
    /// Returns (null, null, null) if the payload isn't a valid message.
    /// </summary>
    public static (string? text, string? from, string? to) ParseTaskRequest(string json)
    {
        try
        {
            Message message = JsonSerializer.Deserialize<Message>(json, s_options)!;
            string? text = message.Parts.OfType<TextPart>().FirstOrDefault()?.Text;
            string? from = message.Metadata != null && message.Metadata.TryGetValue("from", out JsonElement fromElem)
                ? fromElem.GetString()
                : null;
            string? to = message.Metadata != null && message.Metadata.TryGetValue("to", out JsonElement toElem)
                ? toElem.GetString()
                : null;

            return text != null ? (text, from, to) : (null, null, null);
        }
        catch
        {
            return (null, null, null);
        }
    }

    /// <summary>
    /// Parse a capabilities card and return the set of declared skills along with the agent name.
    /// Returns null if the payload isn't a capabilities card.
    /// </summary>
    public static (IList<string>? capabilities, string? from) ParseCapabilityCard(string json)
    {
        try
        {
            AgentCard card = JsonSerializer.Deserialize<AgentCard>(json, s_options)!;
            IList<string> capabilities = card.Skills.Select(skill => skill.Id).ToList();
            return capabilities.Count > 0 ? (capabilities, card.Name) : (null, null);
        }
        catch
        {
            return (null, null);
        }
    }
}

