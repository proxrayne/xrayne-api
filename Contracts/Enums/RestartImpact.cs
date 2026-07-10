using System.Text.Json.Serialization;

namespace Contracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RestartImpact
{
    [JsonStringEnumMemberName("none")]
    None = 0,

    [JsonStringEnumMemberName("hot_reload")]
    HotReload = 1,

    [JsonStringEnumMemberName("full_restart")]
    FullRestart = 2
}
