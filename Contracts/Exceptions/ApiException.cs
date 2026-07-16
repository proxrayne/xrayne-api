namespace Contracts.Exceptions;

public abstract class ApiException : Exception
{
    protected ApiException(int statusCode, string name, string detail)
        : base(detail)
    {
        StatusCode = statusCode;
        Name = name;
        Detail = detail;
    }

    public int StatusCode { get; }

    public string Name { get; }

    public string Detail { get; }
}
