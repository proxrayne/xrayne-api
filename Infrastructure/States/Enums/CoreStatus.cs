using System.Text.Json.Serialization;

namespace Infrastructure.States;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CoreStatus
{
    [JsonStringEnumMemberName("starting")]
    Starting,

    [JsonStringEnumMemberName("started")]
    Started,

    [JsonStringEnumMemberName("stopping")]
    Stopping,

    [JsonStringEnumMemberName("stopped")]
    Stopped,

    [JsonStringEnumMemberName("restarting")]
    Restarting,
}
