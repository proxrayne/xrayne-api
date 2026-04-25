using Microsoft.AspNetCore.Mvc;

namespace XRayne.Api.Controllers;

[ApiController]
[Route("")]
public sealed class HomeController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        return Ok("XRayne API");
    }
}
