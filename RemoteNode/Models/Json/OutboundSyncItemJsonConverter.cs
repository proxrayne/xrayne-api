using System.Text.Json;
using System.Text.Json.Serialization;
using Xray.Config.Models;

namespace RemoteNode.Models.Json;

/// <summary>
/// Reads and writes outbound sync items while isolating xray SDK JSON options.
/// </summary>
public sealed class OutboundSyncItemJsonConverter : JsonConverter<OutboundSyncItem>
{
    /// <inheritdoc />
    public override OutboundSyncItem Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        return new OutboundSyncItem
        {
            Id = XraySyncJson.GetInt64(root, "id"),
            Position = XraySyncJson.GetInt32(root, "position"),
            Outbound = XraySyncJson.GetXray<Outbound>(
                root,
                "outbound",
                "Outbound config cannot be empty.")
        };
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        OutboundSyncItem value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("id", value.Id);
        writer.WriteNumber("position", value.Position);
        XraySyncJson.WriteProperty(writer, "outbound", value.Outbound);
        writer.WriteEndObject();
    }
}
