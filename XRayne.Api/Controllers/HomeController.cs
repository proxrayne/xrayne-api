using Microsoft.AspNetCore.Mvc;

namespace XRayne.Api.Controllers;

[ApiController]
[Route("")]
public sealed class HomeController : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Check API availability")]
    [EndpointDescription("Returns a short text message if the API is running and responding to requests.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public ActionResult<string> Get()
    {
        return Ok("XRayne API");
    }
}
