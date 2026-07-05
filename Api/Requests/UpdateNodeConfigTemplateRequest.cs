using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Request for updating a remote node xray-core configuration template.
/// </summary>
public sealed class UpdateNodeConfigTemplateRequest
{
    /// <summary>
    /// Gets the base xray-core configuration template as JSON text.
    /// </summary>
    [Required]
    public required string ConfigTemplate { get; init; }
}
