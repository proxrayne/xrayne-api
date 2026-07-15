using Xray.Config.Models;
using System.Text.Json.Serialization;
using Node.Models.Json;

namespace Node.Models;

/// <summary>
/// Requests update of the persisted base xray-core configuration template.
/// </summary>
[JsonConverter(typeof(UpdateCoreConfigTemplateRequestJsonConverter))]
public sealed class UpdateCoreConfigTemplateRequest
{
    /// <summary>
    /// Gets the base xray-core configuration template without managed slices.
    /// </summary>
    public required XrayConfig ConfigTemplate { get; init; }
}
