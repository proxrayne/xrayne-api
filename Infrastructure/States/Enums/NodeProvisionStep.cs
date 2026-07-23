using System.Text.Json.Serialization;

namespace Infrastructure.States;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NodeProvisionStep
{
    [JsonStringEnumMemberName("queued")]
    Queued,

    [JsonStringEnumMemberName("preparing")]
    Preparing,

    [JsonStringEnumMemberName("uploading")]
    Uploading,

    [JsonStringEnumMemberName("installing")]
    Installing,

    [JsonStringEnumMemberName("installing_dependencies")]
    InstallingDependencies,

    [JsonStringEnumMemberName("downloading_image")]
    DownloadingImage,

    [JsonStringEnumMemberName("starting_container")]
    StartingContainer,

    [JsonStringEnumMemberName("installing_core")]
    InstallingCore,

    [JsonStringEnumMemberName("verifying")]
    Verifying,

    [JsonStringEnumMemberName("completed")]
    Completed,

    [JsonStringEnumMemberName("failed")]
    Failed
}
