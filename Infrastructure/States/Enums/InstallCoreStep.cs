using System.Text.Json.Serialization;

namespace Infrastructure.States;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InstallCoreStep
{
    [JsonStringEnumMemberName("queued")]
    Queued,

    [JsonStringEnumMemberName("validation")]
    Validation,

    [JsonStringEnumMemberName("downloading")]
    Downloading,

    [JsonStringEnumMemberName("extracting")]
    Extracting,

    [JsonStringEnumMemberName("installing")]
    Installing,

    [JsonStringEnumMemberName("installed")]
    Installed,

    [JsonStringEnumMemberName("failure")]
    Failure,
}
