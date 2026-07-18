using Node.Models;

namespace Node.Services;

/// <summary>
/// Sends authenticated xray-core utility requests to one remote node.
/// </summary>
public interface INodeCoreInfrastructureClient
{
    /// <summary>
    /// Generates X25519 key material on the remote node.
    /// </summary>
    Task<X25519KeysResponse> GetX25519KeysAsync(
        string? privateKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a UUID v5 on the remote node.
    /// </summary>
    Task<string> GetUuidAsync(string input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a UUID v4 on the remote node.
    /// </summary>
    Task<string> GetUuidAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates ML-DSA-65 key material on the remote node.
    /// </summary>
    Task<Mldsa65Response> GetMLDSA65Async(
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates ML-KEM-768 key material on the remote node.
    /// </summary>
    Task<Mlkem768Response> GetMLKEM768Async(
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates VLESS authentication pairs on the remote node.
    /// </summary>
    Task<VlessAuthResponse> GetVlessAuthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates certificate material on the remote node.
    /// </summary>
    Task<GetCertsResponse> GetCertsAsync(
        GetCertsRequest request,
        CancellationToken cancellationToken = default);
}
