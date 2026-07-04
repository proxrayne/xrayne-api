namespace Infrastructure.States;

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
    InstallingCore,
    Verifying,
    Completed,
    Failed
}
