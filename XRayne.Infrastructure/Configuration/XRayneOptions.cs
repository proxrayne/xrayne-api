namespace XRayne.Infrastructure.Configuration;

public sealed class XRayneOptions
{
    public string NodeName { get; set; } = Environment.MachineName;

    public string XrayExecutablePath { get; set; } = "xray";

    public string? XrayServiceName { get; set; } = "xray";
}
