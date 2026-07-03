using Contracts.Models;
using Contracts.Values;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/system")]
[Authorize(Policy = AdminPermissionNames.ViewLogs)]
public sealed class SystemController(ISystemInfoService systemInfoService) : ApiControllerBase
{
    [HttpGet("snapshot")]
    [EndpointSummary("System statistics snapshot")]
    [EndpointDescription("Gets a current system statistics snapshot with CPU, memory, swap, disk, uptime, thread, and network information.")]
    [ProducesResponseType(typeof(SystemInfoSnapshot), StatusCodes.Status200OK)]
    public Task<SystemInfoSnapshot> GetSnapshot(CancellationToken cancellationToken)
    {
        return systemInfoService.GetSnapshotAsync(cancellationToken);
    }
}
