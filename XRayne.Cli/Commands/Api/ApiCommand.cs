using System.CommandLine;

namespace XRayne.Cli.Commands.Api;

public sealed class ApiCommand : Command
{
    public ApiCommand(
        ApiInstallCommand installCommand,
        ApiVersionCommand versionCommand,
        ApiUpdateCommand updateCommand,
        ApiStatusCommand statusCommand,
        ApiStopCommand stopCommand,
        ApiStartCommand startCommand,
        ApiRestartCommand restartCommand)
        : base("api", "Manage XRayne API installation")
    {
        Add(installCommand);
        Add(versionCommand);
        Add(updateCommand);
        Add(statusCommand);
        Add(stopCommand);
        Add(startCommand);
        Add(restartCommand);
    }
}
