using Xray.Config.Enums;
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
    public static XrayConfig Create() => new XrayConfig()
    {
        Log = new LogConfig()
        {
            LogLevel = LogLevel.Warning,
            DnsLog = false,
        },
        Inbounds = new List<Inbound>(),
        Outbounds = new List<Outbound>()
            {
                new FreedomOutbound()
                {
                    Tag = "DIRECT",
                },
                new BlackHoleOutbound()
                {
                    Tag = "BLOCK",
                }
            },
    };
}
