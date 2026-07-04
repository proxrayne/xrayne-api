namespace Infrastructure.States;

public sealed class InstallCoreState
{
    public required InstallCoreStep Step { get; init; }

    public string? Message { get; init; }

    public static InstallCoreState Queued(string version) => new InstallCoreState() { Step = InstallCoreStep.Queued, Message = $"Core installing v{version} scheduled." };
    public static InstallCoreState Validation() => new InstallCoreState() { Step = InstallCoreStep.Validation, Message = "Preparing release and assets validation..." };
    public static InstallCoreState Failure(string message) => new InstallCoreState() { Step = InstallCoreStep.Failure, Message = message };
    public static InstallCoreState Downloading() => new InstallCoreState() { Step = InstallCoreStep.Downloading, Message = "Downloading required asset..." };
    public static InstallCoreState Extracting() => new InstallCoreState() { Step = InstallCoreStep.Extracting, Message = "Extracting downloaded asset..." };
    public static InstallCoreState Installing() => new InstallCoreState() { Step = InstallCoreStep.Installing, Message = "Installing downloaded asset..." };
    public static InstallCoreState Installed(string version) => new InstallCoreState() { Step = InstallCoreStep.Installed, Message = $"Core {version} successful installed." };
}
