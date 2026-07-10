using System.Text.Json;
using System.Text.Json.Serialization;
using Xray.Config.Models;

namespace RemoteNode.Models.Json;

/// <summary>
/// Reads and writes single inbound sync requests while isolating xray SDK JSON options.
/// </summary>
public sealed class SyncInboundRequestJsonConverter : JsonConverter<SyncInboundRequest>
{
    /// <inheritdoc />
    public override SyncInboundRequest Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        return new SyncInboundRequest
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
        SyncInboundRequest value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("id", value.Id);
        writer.WriteNumber("position", value.Position);
        XraySyncJson.WriteProperty(writer, "inbound", value.Inbound);
        writer.WriteEndObject();
    }
}
