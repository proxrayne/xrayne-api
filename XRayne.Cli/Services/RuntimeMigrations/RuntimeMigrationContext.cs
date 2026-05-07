using XRayne.Infrastructure.Values;

namespace XRayne.Cli.Services.RuntimeMigrations;

internal sealed class RuntimeMigrationContext
{
    public RuntimeMigrationContext(ProjectPaths paths)
    {
        Paths = paths;
    }

    public ProjectPaths Paths { get; }
}
