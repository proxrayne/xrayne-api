using System.Text.Json.Serialization;

namespace Contracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubscriptionFormat
{
    [JsonStringEnumMemberName("v2ray")]
    V2Ray,

    [JsonStringEnumMemberName("v2ray_json")]
    V2RayJson,

    [JsonStringEnumMemberName("clash_meta")]
    ClashMeta,

    [JsonStringEnumMemberName("sing_box")]
    SingBox
}
