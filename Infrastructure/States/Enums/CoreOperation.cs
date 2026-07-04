using System.Text.Json.Serialization;

namespace Infrastructure.States;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CoreOperation
{
    [JsonStringEnumMemberName("start")]
    Start,

    [JsonStringEnumMemberName("stop")]
    Stop,

    [JsonStringEnumMemberName("restart")]
    Restart,
}
