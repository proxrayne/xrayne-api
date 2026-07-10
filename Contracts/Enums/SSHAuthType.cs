using System.Text.Json.Serialization;

namespace Contracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SSHAuthType
{
    [JsonStringEnumMemberName("password")]
    Password,

    [JsonStringEnumMemberName("private_key")]
    PrivateKey
}
