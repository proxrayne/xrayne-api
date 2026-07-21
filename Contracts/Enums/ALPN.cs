namespace Contracts.Enums;

/// <summary>
/// Defines ALPN protocol flags stored as a bitmask.
/// </summary>
[Flags]
public enum ALPN
{
    /// <summary>
    /// HTTP/1.1 ALPN protocol.
    /// </summary>
    H1 = 1,

    /// <summary>
    /// HTTP/2 ALPN protocol.
    /// </summary>
    H2 = 2,

    /// <summary>
    /// HTTP/3 ALPN protocol.
    /// </summary>
    H3 = 4
}
