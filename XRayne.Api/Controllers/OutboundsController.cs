using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace XRayne.Api.Controllers;

[Authorize]
[Route("api/outbound")]
public sealed class OutboundsController : ApiControllerBase
{
    
}
