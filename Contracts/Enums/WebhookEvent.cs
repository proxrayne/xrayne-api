using System.Text.Json.Serialization;

namespace Contracts.Enums;

/// <summary>
/// Defines webhook notification events.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
[Flags]
public enum WebhookEvent : ulong
{
    /// <summary>
    /// No webhook events are enabled.
    /// </summary>
    [JsonStringEnumMemberName("none")]
    None = 0,

    /// <summary>
    /// A user was created.
    /// </summary>
    [JsonStringEnumMemberName("user_created")]
    UserCreated = 1UL << 0,

    /// <summary>
    /// A user was updated.
    /// </summary>
    [JsonStringEnumMemberName("user_updated")]
    UserUpdated = 1UL << 1,

    /// <summary>
    /// A user was deleted.
    /// </summary>
    [JsonStringEnumMemberName("user_deleted")]
    UserDeleted = 1UL << 2,

    /// <summary>
    /// A device connected.
    /// </summary>
    [JsonStringEnumMemberName("device_connected")]
    DeviceConnected = 1UL << 3,

    /// <summary>
    /// A device was revoked.
    /// </summary>
    [JsonStringEnumMemberName("device_revoked")]
    DeviceRevoked = 1UL << 4,

    /// <summary>
    /// A user status changed.
    /// </summary>
    [JsonStringEnumMemberName("user_status_changed")]
    UserStatusChanged = 1UL << 5,

    /// <summary>
    /// User traffic was reset.
    /// </summary>
    [JsonStringEnumMemberName("traffic_reset")]
    TrafficReset = 1UL << 6,

    /// <summary>
    /// A traffic usage percentage threshold was reached.
    /// </summary>
    [JsonStringEnumMemberName("traffic_percent_threshold_reached")]
    TrafficPercentThresholdReached = 1UL << 7,

    /// <summary>
    /// A subscription expiration hours threshold was reached.
    /// </summary>
    [JsonStringEnumMemberName("subscription_hours_threshold_reached")]
    SubscriptionHoursThresholdReached = 1UL << 8
}
