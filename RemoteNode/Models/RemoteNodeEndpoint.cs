namespace RemoteNode.Models;

/// <summary>
/// Describes how the panel reaches an authenticated remote node API.
/// </summary>
public sealed record RemoteNodeEndpoint(
    long NodeId,
    string Address,
    int ApiPort,
    string ApiKey);
