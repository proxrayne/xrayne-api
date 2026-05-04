using System.CommandLine;
using XRayne.Cli.Commands.Admin;
using XRayne.Cli.Commands.Xray;

namespace XRayne.Cli.Commands;

public sealed class RootCommandFactory(
    XrayCommand xrayCommand,
    AdminCommand adminCommand,
    VersionCommand versionCommand)
{
    public RootCommand Create()
    {
        return new RootCommand("XRayne CLI")
        {
            xrayCommand,
            adminCommand,
            versionCommand
        };
    }
}
