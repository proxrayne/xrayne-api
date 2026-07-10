using System.Text.Json.Serialization;

namespace Contracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LimitResetStrategy
{
    [JsonStringEnumMemberName("day")]
    Day,

    [JsonStringEnumMemberName("week")]
    Week,

    [JsonStringEnumMemberName("month")]
    Month,

    [JsonStringEnumMemberName("year")]
    Year
}
