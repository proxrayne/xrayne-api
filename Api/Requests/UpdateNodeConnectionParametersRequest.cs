using System.ComponentModel.DataAnnotations;

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
    /// Gets the HTTP/2 API port exposed by the remote node service.
    /// </summary>
    [Range(1, 65535)]
    public int ApiPort { get; init; } = 8443;

    /// <summary>
    /// Gets a replacement node API key. When omitted, the saved API key is kept.
    /// </summary>
    [MaxLength(256)]
    public string? ApiKey { get; init; }
}
