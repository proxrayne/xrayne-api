using System.Text.Json;
using System.Text.Json.Serialization;
using Xray.Config.Models;

namespace RemoteNode.Models.Json;

/// <summary>
/// Reads and writes inbound sync items while isolating xray SDK JSON options.
/// </summary>
public sealed class InboundSyncItemJsonConverter : JsonConverter<InboundSyncItem>
{
    /// <inheritdoc />
    public override InboundSyncItem Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        return new InboundSyncItem
        {
            Id = XraySyncJson.GetInt64(root, "id"),
            Position = XraySyncJson.GetInt32(root, "position"),
            Inbound = XraySyncJson.GetXray<Inbound>(
                root,
                "inbound",
                "Inbound config cannot be empty.")
        };
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        InboundSyncItem value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("id", value.Id);
        writer.WriteNumber("position", value.Position);
        XraySyncJson.WriteProperty(writer, "inbound", value.Inbound);
        writer.WriteEndObject();
    }
}
