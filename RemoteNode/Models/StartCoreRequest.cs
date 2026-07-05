namespace RemoteNode.Models;

/// <summary>
/// Requests xray-core start with a complete runtime configuration.
/// </summary>
public sealed record StartCoreRequest(string Config);
