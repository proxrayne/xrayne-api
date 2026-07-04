using System.Text.Json.Serialization;

namespace Infrastructure.States;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CoreOperationStep
{
    [JsonStringEnumMemberName("queued")]
    Queued,

    [JsonStringEnumMemberName("running")]
    Running,

    [JsonStringEnumMemberName("completed")]
    Completed,

    [JsonStringEnumMemberName("failure")]
    Failure,
}
