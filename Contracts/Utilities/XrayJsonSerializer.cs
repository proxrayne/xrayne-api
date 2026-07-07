using System.Text.Json;
using Xray.Config.Models;

namespace Contracts.Utilities;

/// <summary>
/// Serializes Xray SDK configuration models with the shared SDK JSON options.
/// </summary>
public static class XrayJsonSerializer
{
    /// <summary>
    /// Gets the shared JSON serializer options for Xray SDK configuration models.
    /// </summary>
    public static JsonSerializerOptions Options => XrayConfig.JsonSerializationOptions;

    /// <summary>
    /// Serializes an Xray SDK configuration model.
    /// </summary>
    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, Options);
    }

    /// <summary>
    /// Deserializes an Xray SDK configuration model.
    /// </summary>
    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    /// <summary>
    /// Deserializes an Xray SDK configuration model and rejects empty payloads.
    /// </summary>
    public static T DeserializeRequired<T>(string json, string emptyMessage)
    {
        return Deserialize<T>(json) ?? throw new JsonException(emptyMessage);
    }

    /// <summary>
    /// Clones an Xray SDK configuration model through JSON serialization.
    /// </summary>
    public static T Clone<T>(T value, string emptyMessage)
    {
        var json = Serialize(value);

        return DeserializeRequired<T>(json, emptyMessage);
    }
}
