using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace XRayne.Api.Controllers;

[Authorize]
[Route("api/user")]
public sealed class UsersController : ApiControllerBase
{
    
}
