using System.ComponentModel.DataAnnotations;
using Contracts.Enums;

namespace Api.Requests;

/// <summary>
/// Request for updating saved remote node connection parameters.
/// </summary>
public sealed class UpdateNodeConnectionParametersRequest
{
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
    /// Gets a replacement SSH private key when key authentication is selected.
    /// </summary>
    [MaxLength(512)]
    public string? SSHKey { get; init; }

    /// <summary>
    /// Gets a replacement SSH password when password authentication is selected.
    /// </summary>
    [MaxLength(256)]
    public string? Password { get; init; }
}
