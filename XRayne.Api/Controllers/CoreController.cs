using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using XRayne.Api.Requests;
using XRayne.Api.Responses;
using XRayne.Contracts.Values;
using XRayne.Core.Dto;
using XRayne.Core.Services;
using XRayne.Core.Values;
using XRayne.Infrastructure.Services;
using XRayne.Repositories.External;

namespace XRayne.Api.Controllers;

[Route("api/core")]
public sealed class CoreController(
    ICoreService coreService,
    IMapper mapper,
    IBackgroundTaskScheduler scheduler,
    IMemoryCache cache) : ApiControllerBase
{
    private readonly GitHubRepository xrayRepository = new GitHubRepository(CoreDefaults.XrayRepositoryUrl);

    [HttpGet("status")]
    [EndpointSummary("Core status")]
    [EndpointDescription("Get is actual core status.")]
    [ProducesResponseType(typeof(CoreStatusResponse), StatusCodes.Status200OK)]
    public async Task<CoreStatusResponse> GetStatus()
    {
        return new CoreStatusResponse(
            coreService.GetIsInstalled(),
            coreService.GetIsRunning(),
            coreService.TryGetVersion());
    }

    [HttpGet("releases")]
    [EndpointSummary("Xray releases")]
    [EndpointDescription("Get available Xray releases.")]
    [ProducesResponseType(typeof(List<GitHubReleaseDto>), StatusCodes.Status200OK)]
    public async Task<List<GitHubReleaseDto>> GetReleases([FromQuery] CoreReleasesQuery query, CancellationToken ct)
    {
        var filter = new GithubRepositoriesFilter(query.PerPage, query.Page);
        var releases = await cache.GetOrCreateAsync($"core_releases_{filter.PerPage}_{filter.Page}", entry =>
         {
             entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

             return xrayRepository.GetReleasesAsync(filter, ct);
         });

        if (releases is null)
        {
            throw new Exception("Failed to retrieve releases from cache.");
        }

        return releases.Select(mapper.Map<GitHubReleaseDto>).ToList();
    }

    [HttpGet("install/status")]
    [EndpointSummary("Install Xray status")]
    [EndpointDescription("Get the status of the Xray installation.")]
    [ProducesResponseType(typeof(InstallCoreStatus), StatusCodes.Status200OK)]
    [Authorize(Policy = AdminPermissionNames.SuperAdmin)]
    [Authorize(Policy = AdminPermissionNames.ChangeXraySettings)]
    public async Task<InstallCoreStatus> GetInstallStatus()
    {
        if (cache.TryGetValue(nameof(InstallCoreStatus), out InstallCoreStatus? status) && status is not null)
        {
            return status;
        }

        if (!coreService.GetIsInstalled())
        {
            return new InstallCoreStatus(InstallCoreStep.Idle, "Ready to install.");
        }

        return new InstallCoreStatus(InstallCoreStep.Version, coreService.TryGetVersion() ?? "Unknown");
    }

    [HttpPost("install")]
    [EndpointSummary("Install Xray")]
    [EndpointDescription("Install the specified Xray version.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Authorize(Policy = AdminPermissionNames.SuperAdmin)]
    [Authorize(Policy = AdminPermissionNames.ChangeXraySettings)]
    public async Task<IActionResult> InstallCore([FromBody] InstallCoreRequest data, CancellationToken ct)
    {
        await scheduler.ScheduleInstallCore(data.Version ?? "latest", ct);

        return Created();
    }
}
