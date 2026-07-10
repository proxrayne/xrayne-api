using System.Text.Json;
using System.Text.Json.Serialization;
using Xray.Config.Models;

namespace RemoteNode.Models.Json;

/// <summary>
/// Reads and writes structured core start requests while isolating xray SDK JSON options.
/// </summary>
public sealed class StartCoreRequestJsonConverter : JsonConverter<StartCoreRequest>
{
    /// <inheritdoc />
    public override StartCoreRequest Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        return new StartCoreRequest
        {
            ConfigTemplate = XraySyncJson.GetXray<XrayConfig>(
                root,
                "configTemplate",
                "Core config template cannot be empty."),
            Inbounds = TryReadList<InboundSyncItem>(root, "inbounds", options),
            Outbounds = TryReadList<OutboundSyncItem>(root, "outbounds", options),
            RoutingRules = TryReadList<RoutingRuleSyncItem>(root, "routingRules", options)
        };
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        StartCoreRequest value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        XraySyncJson.WriteProperty(writer, "configTemplate", value.ConfigTemplate);
        writer.WritePropertyName("inbounds");
        JsonSerializer.Serialize(writer, value.Inbounds, options);
        writer.WritePropertyName("outbounds");
        JsonSerializer.Serialize(writer, value.Outbounds, options);
        writer.WritePropertyName("routingRules");
        JsonSerializer.Serialize(writer, value.RoutingRules, options);
        writer.WriteEndObject();
    }

    private static List<T> TryReadList<T>(
        JsonElement root,
        string name,
        JsonSerializerOptions options)
    {
        return root.TryGetProperty(name, out var property)
            ? property.Deserialize<List<T>>(options) ?? []
            : [];
    }
}
