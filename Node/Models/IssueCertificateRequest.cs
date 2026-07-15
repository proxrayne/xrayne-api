namespace Node.Models;

/// <summary>
/// Request to issue a Let's Encrypt certificate on the remote node.
/// </summary>
public sealed record IssueCertificateRequest(string Domain);
