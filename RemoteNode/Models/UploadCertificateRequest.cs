namespace RemoteNode.Models;

/// <summary>
/// Request to import a certificate and private key from remote node file paths.
/// </summary>
public sealed record UploadCertificateRequest(
    string Domain,
    string CertificateFile,
    string PrivateKeyFile);
