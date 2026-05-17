using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace XRayne.Api.Controllers;

[Authorize]
[Route("api/inbound")]
public sealed class InboundsController : ApiControllerBase
{
    
}
