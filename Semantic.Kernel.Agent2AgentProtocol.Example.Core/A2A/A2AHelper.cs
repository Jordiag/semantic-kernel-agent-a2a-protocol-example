using Microsoft.Agents.Core.Models;
using Microsoft.Agents.Core.Serialization;

namespace Semantic.Kernel.Agent2AgentProtocol.Example.Core.A2A;

/// <summary>
/// Helper methods for creating and parsing Activity Protocol payloads.
/// </summary>
public static class A2AHelper
{
    private const string ConversationId = "a2a-demo";

    /// <summary>
    /// Build an Activity message describing this agent's capabilities using a Hero card.
    /// </summary>
    public static string BuildCapabilitiesCard(string from, string to)
    {
        var card = new HeroCard
        {
            Title = "Agent capabilities",
            Buttons =
            [
                new CardAction(type: ActionTypes.MessageBack, title: "reverse", value: "reverse"),
                new CardAction(type: ActionTypes.MessageBack, title: "upper", value: "upper")
            ]
        };

        var activity = new Activity
        {
            Type = ActivityTypes.Message,
            Id = Guid.NewGuid().ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            From = new ChannelAccount(id: from),
            Recipient = new ChannelAccount(id: to),
            Conversation = new ConversationAccount(id: ConversationId),
            Attachments = [card.ToAttachment()]
        };

        return ProtocolJsonSerializer.ToJson(activity);
    }

    /// <summary>
    /// Build an Activity message used to send a text task.
    /// </summary>
    public static string BuildTaskRequest(string text, string from, string to)
    {
        var activity = new Activity
        {
            Type = ActivityTypes.Message,
            Id = Guid.NewGuid().ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            From = new ChannelAccount(id: from),
            Recipient = new ChannelAccount(id: to),
            Conversation = new ConversationAccount(id: ConversationId),
            Text = text
        };

        return ProtocolJsonSerializer.ToJson(activity);
    }

    /// <summary>
    /// Parse an Activity message looking for a text task request.
    /// Returns (null, null, null) if the payload isn't a valid message.
    /// </summary>
    public static (string? text, string? from, string? to) ParseTaskRequest(string json)
    {
        try
        {
            Activity activity = ProtocolJsonSerializer.ToObject<Activity>(json);
            if (activity.Type != ActivityTypes.Message || string.IsNullOrWhiteSpace(activity.Text))
            {
                return (null, null, null);
            }

            return (activity.Text, activity.From?.Id, activity.Recipient?.Id);
        }
        catch
        {
            return (null, null, null);
        }
    }

    /// <summary>
    /// Parse a capabilities card and return the set of declared actions along with the sender id.
    /// Returns null if the payload isn't a capabilities card.
    /// </summary>
    public static (IList<string>? capabilities, string? from) ParseCapabilityCard(string json)
    {
        try
        {
            Activity activity = ProtocolJsonSerializer.ToObject<Activity>(json);
            if (activity.Attachments == null || activity.Attachments.Count == 0)
            {
                return (null, null);
            }

            List<string> capabilities = new();
            foreach (Attachment? attachment in activity.Attachments)
            {
                if (attachment.ContentType == HeroCard.ContentType)
                {
                    HeroCard card = ProtocolJsonSerializer.ToObject<HeroCard>(attachment.Content);
                    foreach (CardAction? button in card.Buttons)
                    {
                        if (button?.Value != null)
                        {
                            capabilities.Add(button.Value.ToString()!);
                        }
                    }
                }
            }

            return capabilities.Count > 0 ? (capabilities, activity.From?.Id) : (null, null);
        }
        catch
        {
            return (null, null);
        }
    }
}

