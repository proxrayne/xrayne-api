using System.ComponentModel.DataAnnotations;
using Contracts.Enums;

namespace Api.Requests;

/// <summary>
/// Request for updating remote node connection and provisioning parameters.
/// </summary>
public sealed class UpdateNodeRequest
{
    /// <summary>
    /// Gets the human-readable node name shown in the panel.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the public domain name or IP address used to reach the node API.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public required string Address { get; init; }

    /// <summary>
    /// Gets the SSH port used during remote provisioning.
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; init; } = 22;

    /// <summary>
    /// Gets the HTTPS API port exposed by the remote node service.
    /// </summary>
    [Range(1, 65535)]
    public int ApiPort { get; init; } = 8443;

    /// <summary>
    /// Gets the SSH username used during remote provisioning.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public required string SSHUsername { get; init; }

    /// <summary>
    /// Gets the SSH authentication method used for provisioning.
    /// </summary>
    public SSHAuthType AuthType { get; init; } = SSHAuthType.Password;

    /// <summary>
    /// Gets the SSH private key used for provisioning when key authentication is selected.
    /// </summary>
    [MaxLength(512)]
    public string? SSHKey { get; init; }

    /// <summary>
    /// Gets the SSH password used for provisioning when password authentication is selected.
    /// </summary>
    [MaxLength(256)]
    public string? Password { get; init; }

    /// <summary>
    /// Gets the remote working directory where XRayne node files are installed.
    /// </summary>
    [Required]
    [MaxLength(256)]
    public required string WorkingDirectory { get; init; }

    /// <summary>
    /// Gets the operator note for the node.
    /// </summary>
    [MaxLength(512)]
    public string Note { get; init; } = string.Empty;
}
