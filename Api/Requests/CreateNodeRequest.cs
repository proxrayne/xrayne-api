using System.ComponentModel.DataAnnotations;
using Contracts.Enums;

namespace Api.Requests;

/// <summary>
/// Request for creating and provisioning a remote node.
/// </summary>
public sealed class CreateNodeRequest
{
    [Required]
    [MaxLength(64)]
    public required string Name { get; init; }

    [Required]
    [MaxLength(64)]
    public required string Address { get; init; }

    [Range(1, 65535)]
    public int Port { get; init; } = 22;

    [Range(1, 65535)]
    public int ApiPort { get; init; } = 8443;

    [Required]
    [MaxLength(64)]
    public required string SSHUsername { get; init; }

    public SSHAuthType AuthType { get; init; } = SSHAuthType.Password;

    [MaxLength(512)]
    public string? SSHKey { get; init; }

    [MaxLength(256)]
    public string? Password { get; init; }

    [Required]
    [MaxLength(256)]
    public required string WorkingDirectory { get; init; }

    [MaxLength(512)]
    public string Note { get; init; } = string.Empty;

}
