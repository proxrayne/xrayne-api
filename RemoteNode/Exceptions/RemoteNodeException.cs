using System.Net;

namespace RemoteNode.Exceptions;

/// <summary>
/// Base exception for intended remote node API failures.
/// </summary>
public abstract class RemoteNodeException : Exception
{
    protected RemoteNodeException(long nodeId, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        NodeId = nodeId;
    }

    /// <summary>
    /// Gets the remote node identifier that produced the failure.
    /// </summary>
    public long NodeId { get; }
}

/// <summary>
/// Represents a remote node network or connectivity failure.
/// </summary>
public sealed class RemoteNodeUnavailableException : RemoteNodeException
{
    public RemoteNodeUnavailableException(long nodeId, string endpoint, Exception innerException)
        : base(nodeId, $"Remote node '{nodeId}' is unavailable while calling '{endpoint}'.", innerException)
    {
        Endpoint = endpoint;
    }

    /// <summary>
    /// Gets the relative endpoint that failed.
    /// </summary>
    public string Endpoint { get; }
}

/// <summary>
/// Represents a remote node request timeout.
/// </summary>
public sealed class RemoteNodeTimeoutException : RemoteNodeException
{
    public RemoteNodeTimeoutException(long nodeId, string endpoint, Exception innerException)
        : base(nodeId, $"Remote node '{nodeId}' timed out while calling '{endpoint}'.", innerException)
    {
        Endpoint = endpoint;
    }

    /// <summary>
    /// Gets the relative endpoint that timed out.
    /// </summary>
    public string Endpoint { get; }
}

/// <summary>
/// Represents a remote node authentication or authorization failure.
/// </summary>
public sealed class RemoteNodeUnauthorizedException : RemoteNodeException
{
    public RemoteNodeUnauthorizedException(long nodeId, string endpoint, HttpStatusCode statusCode)
        : base(nodeId, $"Remote node '{nodeId}' rejected credentials for '{endpoint}' with status {(int)statusCode}.")
    {
        Endpoint = endpoint;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the relative endpoint that failed.
    /// </summary>
    public string Endpoint { get; }

    /// <summary>
    /// Gets the HTTP status code returned by the node.
    /// </summary>
    public HttpStatusCode StatusCode { get; }
}

/// <summary>
/// Represents a non-success remote node HTTP response.
/// </summary>
public sealed class RemoteNodeHttpException : RemoteNodeException
{
    public RemoteNodeHttpException(long nodeId, string endpoint, HttpStatusCode statusCode, string? responseBody)
        : base(nodeId, $"Remote node '{nodeId}' returned status {(int)statusCode} for '{endpoint}'.")
    {
        Endpoint = endpoint;
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    /// <summary>
    /// Gets the relative endpoint that failed.
    /// </summary>
    public string Endpoint { get; }

    /// <summary>
    /// Gets the HTTP status code returned by the node.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets a bounded response body for diagnostics.
    /// </summary>
    public string? ResponseBody { get; }
}

/// <summary>
/// Represents an invalid or unexpected remote node protocol response.
/// </summary>
public sealed class RemoteNodeProtocolException : RemoteNodeException
{
    public RemoteNodeProtocolException(long nodeId, string endpoint, string detail, Exception? innerException = null)
        : base(nodeId, $"Remote node '{nodeId}' returned an invalid response for '{endpoint}': {detail}", innerException)
    {
        Endpoint = endpoint;
        Detail = detail;
    }

    /// <summary>
    /// Gets the relative endpoint that returned invalid data.
    /// </summary>
    public string Endpoint { get; }

    /// <summary>
    /// Gets the protocol failure detail.
    /// </summary>
    public string Detail { get; }
}
