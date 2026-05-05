using Microsoft.Extensions.Configuration;

namespace XRayne.Cli.Services;

public sealed class XRayneEnvironmentConfigurationSource : IConfigurationSource
{
    public const string DefaultEnvironmentFilePath = "/opt/xrayne/.env";
    public const string EnvironmentFilePathVariable = "XRAYNE_ENV_FILE";

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var path = Environment.GetEnvironmentVariable(EnvironmentFilePathVariable);
        if (string.IsNullOrWhiteSpace(path))
        {
            path = DefaultEnvironmentFilePath;
        }

        return new XRayneEnvironmentConfigurationProvider(path);
    }
}
