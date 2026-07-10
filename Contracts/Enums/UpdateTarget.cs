using System.Text.Json.Serialization;

namespace Contracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UpdateTarget
{
    [JsonStringEnumMemberName("json")]
    Json,

    [JsonStringEnumMemberName("env")]
    Env
}
