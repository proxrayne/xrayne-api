using System.CommandLine;
using XRayne.Cli.Commands.Api;
using XRayne.Cli.Commands.Admin;
using XRayne.Cli.Commands.Xray;

namespace XRayne.Cli.Commands;

public sealed class RootCommandFactory(
    XrayCommand xrayCommand,
    AdminCommand adminCommand,
    ApiCommand apiCommand,
    VersionCommand versionCommand,
    UpdateCommand updateCommand,
    InfoCommand infoCommand)
{
    public RootCommand Create()
    {
        return new RootCommand("XRayne CLI")
        {
            xrayCommand,
            adminCommand,
            apiCommand,
            versionCommand,
            updateCommand,
            infoCommand
        };
    }
}
