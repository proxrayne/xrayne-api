using Contracts.Enums;

namespace Contracts.Models;

/// <summary>
/// Stores live remote node connection state.
/// </summary>
public sealed record NodeConnectionState(
    long NodeId,
    NodeConnectionStatus Status,
    string? ApiVersion,
    DateTimeOffset? Uptime);
