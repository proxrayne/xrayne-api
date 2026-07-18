namespace Node.Models;

/// <summary>
/// Describes generated X25519 key material from a remote node.
/// </summary>
public sealed record X25519KeysResponse(
    string PrivateKey,
    string Password,
    string Hash);
