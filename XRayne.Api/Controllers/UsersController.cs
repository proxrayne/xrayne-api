using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace XRayne.Api.Controllers;

[Authorize]
[Route("api/users")]
public sealed class UsersController : ApiControllerBase
{
    
}
