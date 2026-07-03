using Microsoft.AspNetCore.Mvc;
using Node.Responses;

namespace Node.Controllers;

[ApiController]
[Route("api/core")]
public sealed class CoreController : ControllerBase
{
    [HttpGet("status")]
    [EndpointSummary("Core status")]
    [EndpointDescription("Get the current Xray core status placeholder.")]
    [ProducesResponseType(typeof(CoreStatusResponse), StatusCodes.Status200OK)]
    public CoreStatusResponse GetStatus()
    {
        return new CoreStatusResponse(false, false, null, "not_implemented");
    }

    [HttpPost("install")]
    [EndpointSummary("Install Xray")]
    [EndpointDescription("Schedule Xray core installation placeholder.")]
    [ProducesResponseType(typeof(InstallCoreResponse), StatusCodes.Status202Accepted)]
    public ActionResult<InstallCoreResponse> Install([FromBody] InstallCoreRequest request)
    {
        return Accepted(new InstallCoreResponse(Guid.NewGuid().ToString("N"), request.Version ?? "latest", "not_implemented"));
    }

    [HttpGet("install/{jobId}/status")]
    [EndpointSummary("Install Xray status")]
    [EndpointDescription("Get Xray core installation status placeholder.")]
    [ProducesResponseType(typeof(InstallCoreStatusResponse), StatusCodes.Status200OK)]
    public InstallCoreStatusResponse GetInstallStatus(string jobId)
    {
        return new InstallCoreStatusResponse(jobId, "not_implemented", null);
    }

    [HttpPost("start")]
    [EndpointSummary("Start Xray")]
    [EndpointDescription("Schedule Xray core start placeholder.")]
    [ProducesResponseType(typeof(OperationAcceptedResponse), StatusCodes.Status202Accepted)]
    public ActionResult<OperationAcceptedResponse> Start()
    {
        return Accepted(new OperationAcceptedResponse("start", "not_implemented"));
    }

    [HttpPost("stop")]
    [EndpointSummary("Stop Xray")]
    [EndpointDescription("Schedule Xray core stop placeholder.")]
    [ProducesResponseType(typeof(OperationAcceptedResponse), StatusCodes.Status202Accepted)]
    public ActionResult<OperationAcceptedResponse> Stop()
    {
        return Accepted(new OperationAcceptedResponse("stop", "not_implemented"));
    }

    [HttpPost("restart")]
    [EndpointSummary("Restart Xray")]
    [EndpointDescription("Schedule Xray core restart placeholder.")]
    [ProducesResponseType(typeof(OperationAcceptedResponse), StatusCodes.Status202Accepted)]
    public ActionResult<OperationAcceptedResponse> Restart()
    {
        return Accepted(new OperationAcceptedResponse("restart", "not_implemented"));
    }
}
