using Github;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Quartz;
using Contracts.Values;
using Infrastructure.Services;
using Infrastructure.States;
using Infrastructure.Utilities;
using Infrastructure.Values;

namespace Infrastructure.Tasks;

[DisallowConcurrentExecution]
public sealed class InstallCoreJob(
    ICoreService coreService,
    ICoreStateMachine stateMachine,
    ILogger<InstallCoreJob> logger) : IJob
{
    public const string VersionKey = "version";
    public const string IdentityKey = "id";

    public static readonly JobKey JobKey = new(nameof(InstallCoreJob), "core-install");
    public static readonly TriggerKey TriggerKey = new(nameof(InstallCoreJob), "core-install");

    private readonly GitHubRepository xrayRepository = new(CoreDefaults.XrayRepositoryUrl);

    public async Task Execute(IJobExecutionContext context)
    {
        var jobId = context.MergedJobDataMap.GetString(IdentityKey)!;

        try
        {
            var version = context.MergedJobDataMap.GetString(VersionKey) ?? CoreDefaults.DefaultVersion;

            stateMachine.DispatchInstallState(jobId, InstallCoreState.Validation());

            var release = await xrayRepository.GetReleaseAsync(version, context.CancellationToken);
            var localAssetName = $"xray-{release.TagName.Replace(".", "_")}";
            var assetName = CoreReleasesUtilities.GetCurrentPlatformAssetName();
            var asset = release.Assets.FirstOrDefault(item =>
                string.Equals(item.Name, assetName, StringComparison.OrdinalIgnoreCase));
            if (asset is null)
            {
                throw new InvalidOperationException($"Release '{release.TagName}' does not contain asset '{assetName}'.");
            }

            stateMachine.DispatchInstallState(jobId, InstallCoreState.Downloading());

            var downloadsDirectory = Path.Combine(PathProvider.Paths.DownloadsDirectory, "xray-core");
            var destinationPath = await xrayRepository.DownloadAssetAsync(
                asset,
                downloadsDirectory,
                $"{localAssetName}.zip",
                context.CancellationToken);

            stateMachine.DispatchInstallState(jobId, InstallCoreState.Extracting());

            ZipFile.ExtractToDirectory(
                destinationPath,
                Path.Combine(PathProvider.Paths.XrayDirectory, localAssetName),
                overwriteFiles: true);
            File.Delete(destinationPath);

            stateMachine.DispatchInstallState(jobId, InstallCoreState.Installing());

            await coreService.SetupAsync(localAssetName, context.CancellationToken);

            stateMachine.DispatchInstallState(jobId, InstallCoreState.Installed(version));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Core installation failed.");
            stateMachine.DispatchInstallState(jobId, InstallCoreState.Failure(ex.Message));
        }
    }
}
