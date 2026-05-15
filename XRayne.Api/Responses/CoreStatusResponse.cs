using XRayne.Infrastructure.States;

namespace XRayne.Api.Responses;

public sealed record CoreStatusResponse(bool IsInstalled, bool IsStarted, string? Version, InstallCoreState? InstallStatus);
