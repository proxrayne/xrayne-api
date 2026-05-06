using System.CommandLine;
using XRayne.Cli.Commands.Api;
using XRayne.Cli.Commands.Admin;
using XRayne.Cli.Commands.Cert;
using XRayne.Cli.Commands.Xray;

namespace XRayne.Cli.Commands;

public sealed class RootCommandFactory(
    XrayCommand xrayCommand,
    AdminCommand adminCommand,
    ApiCommand apiCommand,
    CertCommand certCommand,
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
            certCommand,
            versionCommand,
            updateCommand,
            infoCommand
        };
    }
}
