using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace XRayne.Api.Controllers;

[Authorize]
[Route("api/inbounds")]
public sealed class InboundsController : ApiControllerBase
{
    
}
