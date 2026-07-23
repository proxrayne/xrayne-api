using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Request for creating a remote node connection.
/// </summary>
public sealed class CreateNodeRequest
{
    /// <summary>
    /// Gets the human-readable node name.
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
    /// Gets the HTTP/2 node API port.
    /// </summary>
    [Range(1, 65535)]
    public int ApiPort { get; init; } = 8443;

    /// <summary>
    /// Gets the node API key used to authenticate direct panel-to-node calls.
    /// </summary>
    [Required]
    [MaxLength(256)]
    public required string ApiKey { get; init; }

    /// <summary>
    /// Gets an optional operator note.
    /// </summary>
    [MaxLength(512)]
    public string Note { get; init; } = string.Empty;
}
