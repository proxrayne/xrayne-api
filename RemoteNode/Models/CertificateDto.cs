namespace RemoteNode.Models;

/// <summary>
/// Remote node certificate metadata response.
/// </summary>
public sealed record CertificateDto(
    string Domain,
    bool Active,
    DateTimeOffset ExpireAt,
    string CertificateFile,
    string PrivateKeyFile);
