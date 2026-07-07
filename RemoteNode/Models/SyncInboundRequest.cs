namespace RemoteNode.Models;

/// <summary>
/// Carries one inbound JSON configuration to the remote node runtime.
/// </summary>
public sealed record SyncInboundRequest(string Config);
