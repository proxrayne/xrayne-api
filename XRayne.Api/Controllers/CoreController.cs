using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using XRayne.Api.Exceptions;
using XRayne.Api.Requests;
using XRayne.Api.Responses;
using XRayne.Contracts.Values;
using XRayne.Core.Services;
using XRayne.Core.States;
using XRayne.Core.Values;
using XRayne.Infrastructure.Services;
using XRayne.Repositories.External;

namespace XRayne.Api.Controllers;

[Route("api/core")]
[Authorize(Policy = AdminPermissionNames.SuperAdmin)]
[Authorize(Policy = AdminPermissionNames.ChangeXraySettings)]
public sealed class CoreController(
    ICoreService coreService,
    IMapper mapper,
    IBackgroundTaskScheduler scheduler,
    ICoreStateMachine coreState,
    IMemoryCache cache) : ApiControllerBase
{
    private readonly GitHubRepository xrayRepository = new GitHubRepository(CoreDefaults.XrayRepositoryUrl);

    [HttpGet("status")]
    [EndpointSummary("Core status")]
    [EndpointDescription("Get is actual core status.")]
    [ProducesResponseType(typeof(CoreStatusResponse), StatusCodes.Status200OK)]
    public async Task<CoreStatusResponse> GetStatus()
    {
        var installingStatus = coreState.GetInstallCoreState();

        return new CoreStatusResponse(
            coreService.GetIsInstalled(),
            coreService.GetIsRunning(),
            coreService.TryGetVersion(),
            installingStatus);
    }

    [HttpGet("releases")]
    [EndpointSummary("Xray releases")]
    [EndpointDescription("Get available Xray releases.")]
    [ProducesResponseType(typeof(List<GitHubReleaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<ApiErrorResponse>), StatusCodes.Status400BadRequest)]
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

    [HttpGet("install/{jobId}/status")]
    [EndpointSummary("Install Xray status")]
    [EndpointDescription("Get the status of the Xray installation.")]
    [ProducesResponseType(typeof(InstallCoreState), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]

    public async Task<IActionResult> GetInstallState(string jobId)
    {
        var state = coreState.GetInstallCoreState(jobId);
        if (state is null)
        {
            throw new NotFoundException($"Core by JobId = {jobId} not found.");
        }

        return Ok(state);
    }

    [HttpPost("install")]
    [EndpointSummary("Install Xray")]
    [EndpointDescription("Install the specified Xray version.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<string> InstallCore([FromBody] InstallCoreRequest data, CancellationToken ct)
    {
        return scheduler.ScheduleInstallCore(data.Version ?? "latest", ct);
    }
}
