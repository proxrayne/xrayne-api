using Xray.Config.Models;

namespace Contracts.Values;

/// <summary>
/// Provides default Xray configuration templates for remote nodes.
/// </summary>
public static class NodeConfigTemplateDefaults
{
    /// <summary>
    /// Creates the default remote node xray-core configuration template.
    /// </summary>
    public static XrayConfig Create()
    {
        return XrayConfig.FromJson("""{"log":{"loglevel":"warning"}}""");
    }
}
