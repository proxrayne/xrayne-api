using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace XRayne.Contracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum XrayUsageMode
{
    [EnumMember(Value = "process")]
    Process,

    [EnumMember(Value = "library")]
    Library
}