using System.Text.Json;
using System.Text.Json.Serialization;
using Xray.Config.Models;

namespace RemoteNode.Models.Json;

/// <summary>
/// Reads and writes routing rule sync items while isolating xray SDK JSON options.
/// </summary>
public sealed class RoutingRuleSyncItemJsonConverter : JsonConverter<RoutingRuleSyncItem>
{
    /// <inheritdoc />
    public override RoutingRuleSyncItem Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        return new RoutingRuleSyncItem
        {
            Id = XraySyncJson.GetInt64(root, "id"),
            Position = XraySyncJson.GetInt32(root, "position"),
            RoutingRule = XraySyncJson.GetXray<RoutingRule>(
                root,
                "routingRule",
                "Routing rule config cannot be empty.")
        };
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        RoutingRuleSyncItem value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("id", value.Id);
        writer.WriteNumber("position", value.Position);
        XraySyncJson.WriteProperty(writer, "routingRule", value.RoutingRule);
        writer.WriteEndObject();
    }
}
