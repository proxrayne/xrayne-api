using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/user")]
public sealed class UsersController : ApiControllerBase
{
    
}
