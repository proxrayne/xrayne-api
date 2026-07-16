using Microsoft.AspNetCore.Http;

namespace Contracts.Exceptions;

public sealed class ForbiddenException : ApiException
{
    public ForbiddenException(string detail)
        : base(StatusCodes.Status403Forbidden, "Forbidden", detail)
    {
    }
}
