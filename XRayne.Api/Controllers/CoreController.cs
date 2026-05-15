using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using XRayne.Api.Exceptions;
using XRayne.Api.Requests;
using XRayne.Api.Responses;
using XRayne.Contracts.Values;
using XRayne.Infrastructure.Services;
using XRayne.Infrastructure.States;
using XRayne.Infrastructure.Values;
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
    IEventStreamManager eventStreams,
    IMemoryCache cache) : ApiControllerBase
{
    private readonly GitHubRepository xrayRepository = new GitHubRepository(CoreDefaults.XrayRepositoryUrl);
    private static readonly JsonSerializerOptions SseJsonOptions = new(JsonSerializerDefaults.Web);

    [HttpGet("status")]
    [EndpointSummary("Core status")]
    [EndpointDescription("Get is actual core status.")]
    [ProducesResponseType(typeof(CoreStatusResponse), StatusCodes.Status200OK)]
    public CoreStatusResponse GetStatus()
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

    public IActionResult GetInstallState(string jobId)
    {
        var state = coreState.GetInstallCoreState(jobId);
        if (state is null)
        {
            throw new NotFoundException($"Core by JobId = {jobId} not found.");
        }

        return Ok(state);
    }

    [HttpGet("install/{jobId}/stream")]
    [EndpointSummary("Install Xray state stream")]
    [EndpointDescription("Subscribe to Xray installation state changes.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task StreamInstallState(string jobId, CancellationToken ct)
    {
        var currentState = coreState.GetInstallCoreState(jobId);
        if (currentState is null)
        {
            throw new NotFoundException($"Core by JobId = {jobId} not found.");
        }

        var streamKey = CoreStateMachine.GetInstallCoreStreamKey(jobId);
        var subscription = eventStreams.Subscribe<InstallCoreState>(streamKey);

        Response.ContentType = "text/event-stream; charset=utf-8";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";
        Response.Headers["X-Accel-Buffering"] = "no";

        try
        {
            await Response.StartAsync(ct);
            await WriteServerSentEventAsync(currentState, ct);

            await foreach (var state in subscription.Reader.ReadAllAsync(ct))
            {
                await WriteServerSentEventAsync(state, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        finally
        {
            eventStreams.Unsubscribe(subscription.Id);
        }
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

    private async Task WriteServerSentEventAsync<T>(T data, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(data, SseJsonOptions);

        await Response.WriteAsync($"data: {json}\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }
}
