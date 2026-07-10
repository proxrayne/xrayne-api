using System.Text.Json.Serialization;

namespace Contracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserStatus
{
    [JsonStringEnumMemberName("active")]
    Active,

    [JsonStringEnumMemberName("expired")]
    Expired,

    [JsonStringEnumMemberName("limited")]
    Limited,

    [JsonStringEnumMemberName("on_hold")]
    OnHold,

    [JsonStringEnumMemberName("disabled")]
    Disabled,
}
