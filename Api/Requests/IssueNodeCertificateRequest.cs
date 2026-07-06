namespace Api.Requests;

/// <summary>
/// Request to issue a certificate on a remote node.
/// </summary>
public sealed record IssueNodeCertificateRequest(string Domain);
