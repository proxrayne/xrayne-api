using System.Text.Json.Serialization;
using XRayne.Contracts.Configurations;

namespace XRayne.Api.Responses;

public sealed class PanelSettingsResponse
{
    public string? BindIp { get; set; }

    public string? Domain { get; set; }

    public int Port { get; set; }

    public string WebBasePath { get; set; } = "/";

    public int SessionLifetimeMinutes { get; set; }

    public string? TrustedProxyCidrs { get; set; }

    public string? CertificatesDirectory { get; set; }

    public string? GeoResourcesDirectory { get; set; }

    public string? PanelCertPublicKeyPath { get; set; }

    public string? PanelCertPrivateKeyPath { get; set; }

    public bool PendingRestart { get; set; }

    [JsonConverter(typeof(FieldImpactsConverter))]
    public Dictionary<string, RestartImpact> FieldImpacts { get; set; } = new();
}

internal sealed class FieldImpactsConverter : JsonConverter<Dictionary<string, RestartImpact>>
{
    private static readonly JsonConverter<RestartImpact> EnumConverter =
        new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase)
            .CreateConverter(typeof(RestartImpact), new System.Text.Json.JsonSerializerOptions())
            as JsonConverter<RestartImpact>
        ?? throw new InvalidOperationException("Failed to build RestartImpact converter.");

    public override Dictionary<string, RestartImpact> Read(
        ref System.Text.Json.Utf8JsonReader reader,
        Type typeToConvert,
        System.Text.Json.JsonSerializerOptions options)
    {
        var result = new Dictionary<string, RestartImpact>();
        if (reader.TokenType != System.Text.Json.JsonTokenType.StartObject)
        {
            throw new System.Text.Json.JsonException("Expected object.");
        }

        while (reader.Read())
        {
            if (reader.TokenType == System.Text.Json.JsonTokenType.EndObject)
            {
                return result;
            }

            var key = reader.GetString()!;
            reader.Read();
            result[key] = EnumConverter.Read(ref reader, typeof(RestartImpact), options);
        }

        throw new System.Text.Json.JsonException("Unexpected end of JSON.");
    }

    public override void Write(
        System.Text.Json.Utf8JsonWriter writer,
        Dictionary<string, RestartImpact> value,
        System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var (key, impact) in value)
        {
            writer.WritePropertyName(key);
            EnumConverter.Write(writer, impact, options);
        }
        writer.WriteEndObject();
    }
}
