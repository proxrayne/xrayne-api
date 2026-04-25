using Microsoft.AspNetCore.Mvc;

namespace XRayne.Api.Controllers;

[ApiController]
[Route("api/core")]
public sealed class CoreController : Controller
{
    [HttpPost("start")]
    public async Task<IActionResult> StartXray() {
        

        return Ok("started");
    }   
}