namespace XRayne.Infrastructure.States;

public enum NodeProvisionStep
{
    Queued,
    Preparing,
    Uploading,
    Installing,
    InstallingDependencies,
    DownloadingImage,
    ConfiguringCertificate,
    StartingContainer,
    Verifying,
    Completed,
    Failed
}

public sealed record NodeProvisionState(
    long NodeId,
    string JobId,
    NodeProvisionStep Step,
    string Message,
    DateTimeOffset UpdatedAt)
{
    public static NodeProvisionState Queued(long nodeId, string jobId)
        => new(nodeId, jobId, NodeProvisionStep.Queued, "Node provisioning is queued.", DateTimeOffset.UtcNow);

    public static NodeProvisionState Preparing(long nodeId, string jobId)
        => new(nodeId, jobId, NodeProvisionStep.Preparing, "Preparing remote node installation.", DateTimeOffset.UtcNow);

    public static NodeProvisionState Uploading(long nodeId, string jobId)
        => new(nodeId, jobId, NodeProvisionStep.Uploading, "Uploading remote node installer.", DateTimeOffset.UtcNow);

    public static NodeProvisionState Installing(long nodeId, string jobId)
        => new(nodeId, jobId, NodeProvisionStep.Installing, "Installing remote node service.", DateTimeOffset.UtcNow);

    public static NodeProvisionState InstallingDependencies(long nodeId, string jobId)
        => new(nodeId, jobId, NodeProvisionStep.InstallingDependencies, "Installing remote dependencies.", DateTimeOffset.UtcNow);

    public static NodeProvisionState DownloadingImage(long nodeId, string jobId)
        => new(nodeId, jobId, NodeProvisionStep.DownloadingImage, "Downloading remote node image.", DateTimeOffset.UtcNow);

    public static NodeProvisionState ConfiguringCertificate(long nodeId, string jobId)
        => new(nodeId, jobId, NodeProvisionStep.ConfiguringCertificate, "Configuring remote node certificate.", DateTimeOffset.UtcNow);

    public static NodeProvisionState StartingContainer(long nodeId, string jobId)
        => new(nodeId, jobId, NodeProvisionStep.StartingContainer, "Starting remote node container.", DateTimeOffset.UtcNow);

    public static NodeProvisionState Verifying(long nodeId, string jobId)
        => new(nodeId, jobId, NodeProvisionStep.Verifying, "Verifying remote node connection.", DateTimeOffset.UtcNow);

    public static NodeProvisionState Completed(long nodeId, string jobId)
        => new(nodeId, jobId, NodeProvisionStep.Completed, "Remote node is connected.", DateTimeOffset.UtcNow);

    public static NodeProvisionState Failed(long nodeId, string jobId, string message)
        => new(nodeId, jobId, NodeProvisionStep.Failed, message, DateTimeOffset.UtcNow);
}
