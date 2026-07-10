using System.Text.Json;
using Xray.Config.Models;

namespace RemoteNode.Models.Json;

internal static class XraySyncJson
{
    public static JsonElement GetProperty(JsonElement root, string name)
    {
        if (root.TryGetProperty(name, out var property))
        {
            return property;
        }

        foreach (var item in root.EnumerateObject())
        {
            if (string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return item.Value;
            }
        }

        throw new JsonException($"Property '{name}' is required.");
    }

    public static long GetInt64(JsonElement root, string name)
    {
        return GetProperty(root, name).GetInt64();
    }

    public static int GetInt32(JsonElement root, string name)
    {
        return GetProperty(root, name).GetInt32();
    }

    public static T GetXray<T>(JsonElement root, string name, string emptyMessage)
    {
        return JsonSerializer.Deserialize<T>(
                GetProperty(root, name).GetRawText(),
                XrayConfig.JsonSerializationOptions)
            ?? throw new JsonException(emptyMessage);
    }

    public static void WriteProperty<T>(
        Utf8JsonWriter writer,
        string name,
        T value)
    {
        writer.WritePropertyName(name);
        JsonSerializer.Serialize(writer, value, XrayConfig.JsonSerializationOptions);
    }
}
