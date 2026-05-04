using System.CommandLine;

namespace XRayne.Cli.Commands.Api;

public sealed class ApiCommand : Command
{
    public ApiCommand(ApiInstallCommand installCommand)
        : base("api", "Manage XRayne API installation")
    {
        Add(installCommand);
    }
}
