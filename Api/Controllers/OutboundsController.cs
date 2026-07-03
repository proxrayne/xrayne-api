using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/outbound")]
public sealed class OutboundsController : ApiControllerBase
{
    
}
