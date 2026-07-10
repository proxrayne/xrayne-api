using System.Text.Json;
using System.Text.Json.Serialization;
using Xray.Config.Models;

namespace RemoteNode.Models.Json;

/// <summary>
/// Reads and writes single outbound sync requests while isolating xray SDK JSON options.
/// </summary>
public sealed class SyncOutboundRequestJsonConverter : JsonConverter<SyncOutboundRequest>
{
    /// <inheritdoc />
    public override SyncOutboundRequest Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        return new SyncOutboundRequest
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
        SyncOutboundRequest value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("id", value.Id);
        writer.WriteNumber("position", value.Position);
        XraySyncJson.WriteProperty(writer, "outbound", value.Outbound);
        writer.WriteEndObject();
    }
}
