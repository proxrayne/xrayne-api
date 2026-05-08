using System.IO.Compression;
using Microsoft.Extensions.Caching.Memory;
using Quartz;
using XRayne.Contracts.Values;
using XRayne.Core.Dto;
using XRayne.Core.Services;
using XRayne.Core.Utilities;
using XRayne.Core.Values;
using XRayne.Repositories.External;

namespace XRayne.Core.Tasks;

[DisallowConcurrentExecution]
public sealed class InstallCoreJob(ICoreService coreService, IMemoryCache cache) : IJob
{
    public const string VersionKey = "version";

    public static readonly JobKey JobKey = new JobKey(nameof(InstallCoreJob), "core");
    public static readonly TriggerKey TriggerKey = new TriggerKey(nameof(InstallCoreJob), "core");

    private readonly GitHubRepository xrayRepository = new(CoreDefaults.XrayRepositoryUrl);

    public async Task Execute(IJobExecutionContext context)
    {
        if (cache.TryGetValue(nameof(InstallCoreStatus), out InstallCoreStatus? status) && status is not null)
        {
            throw new InvalidOperationException("Core installation is already in progress.");
        }

        UpdateStatus(InstallCoreStep.Preparing, "Preparing installation...");

        var version = context.MergedJobDataMap.GetString(VersionKey) ?? "latest";
        var release = await xrayRepository.GetReleaseAsync(version, context.CancellationToken);
        var assetName = CoreReleasesUtilities.GetCurrentPlatformAssetName();
        var asset = release.Assets.FirstOrDefault(item =>
            string.Equals(item.Name, assetName, StringComparison.OrdinalIgnoreCase));
        if (asset is null)
        {
            UpdateStatus(InstallCoreStep.Failure, "Release does not contain the required asset.");

            throw new InvalidOperationException($"Release '{release.TagName}' does not contain asset '{assetName}'.");
        }

        UpdateStatus(InstallCoreStep.Downloading, "Downloading required asset.");

        var downloadsDirectory = Path.Combine(PathProvider.Paths.DownloadsDirectory, "xray-core");
        var localAssetName = $"xray-{release.TagName.Replace(".", "_")}";
        var destinationPath = await xrayRepository.DownloadAssetAsync(asset, downloadsDirectory, $"{localAssetName}.zip", context.CancellationToken);

        UpdateStatus(InstallCoreStep.Extracting, "Extracting downloaded asset.");

        ZipFile.ExtractToDirectory(destinationPath, Path.Combine(PathProvider.Paths.XrayDirectory, localAssetName));
        File.Delete(destinationPath);

        UpdateStatus(InstallCoreStep.SettingUp, "Setting up core...");

        await coreService.SetupAsync(localAssetName);

        cache.Remove(nameof(InstallCoreStatus));
    }

    private void UpdateStatus(InstallCoreStep step, string message)
    {
        cache.Set(nameof(InstallCoreStatus), new InstallCoreStatus(step, message), TimeSpan.FromHours(2));
    }
}
