using System.Text.Json;
using System.Text.Json.Serialization;

namespace RemoteNode.Models.Json;

/// <summary>
/// Reads and writes routing rule batch sync requests.
/// </summary>
public sealed class SyncRoutingRulesRequestJsonConverter : JsonConverter<SyncRoutingRulesRequest>
{
    /// <inheritdoc />
    public override SyncRoutingRulesRequest Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        var rules = XraySyncJson.GetProperty(root, "routingRules")
            .Deserialize<List<RoutingRuleSyncItem>>(options) ?? [];

        return new SyncRoutingRulesRequest
        {
            RoutingRules = rules
        };
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        SyncRoutingRulesRequest value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("routingRules");
        JsonSerializer.Serialize(writer, value.RoutingRules, options);
        writer.WriteEndObject();
    }
}
