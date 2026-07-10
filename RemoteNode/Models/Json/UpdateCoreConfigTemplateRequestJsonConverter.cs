using System.Text.Json;
using System.Text.Json.Serialization;
using Xray.Config.Models;

namespace RemoteNode.Models.Json;

/// <summary>
/// Reads and writes core template update requests while isolating xray SDK JSON options.
/// </summary>
public sealed class UpdateCoreConfigTemplateRequestJsonConverter : JsonConverter<UpdateCoreConfigTemplateRequest>
{
    /// <inheritdoc />
    public override UpdateCoreConfigTemplateRequest Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        return new UpdateCoreConfigTemplateRequest
        {
            ConfigTemplate = XraySyncJson.GetXray<XrayConfig>(
                root,
                "configTemplate",
                "Core config template cannot be empty.")
        };
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        UpdateCoreConfigTemplateRequest value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        XraySyncJson.WriteProperty(writer, "configTemplate", value.ConfigTemplate);
        writer.WriteEndObject();
    }
}
