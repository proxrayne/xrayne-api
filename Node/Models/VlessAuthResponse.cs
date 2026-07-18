namespace Node.Models;

/// <summary>
/// Describes generated VLESS authentication pairs from a remote node.
/// </summary>
public sealed record VlessAuthResponse(
    VlessAuthPair X25519,
    VlessAuthPair Mlkem768);

/// <summary>
/// Describes one VLESS authentication key pair.
/// </summary>
public sealed record VlessAuthPair(
    string Decryption,
    string Encryption);