namespace XRayne.Api.Exceptions;

public sealed class NotFoundException : ApiException
{
    public NotFoundException(string detail)
        : base(StatusCodes.Status404NotFound, "Not Found", detail)
    {
    }
}
