using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace XRayne.Core.Dto;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InstallCoreStep
{
    [EnumMember(Value = "idle")]
    Idle,
    
    [EnumMember(Value = "preparing")]
    Preparing,

    [EnumMember(Value = "failure")]
    Failure,

    [EnumMember(Value = "downloading")]
    Downloading,

    [EnumMember(Value = "extracting")]
    Extracting,

    [EnumMember(Value = "setting-up")]
    SettingUp
}

public sealed record InstallCoreStatus(InstallCoreStep Step, string Message);