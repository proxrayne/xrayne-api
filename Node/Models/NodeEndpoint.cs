namespace Node.Models;

/// <summary>
/// Describes how the panel reaches an authenticated remote node API.
/// </summary>
public sealed record NodeEndpoint(
    long NodeId,
    string Address,
    int ApiPort,
    string ApiKey);
