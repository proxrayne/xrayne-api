namespace Node.Values;

/// <summary>
/// Defines default geo resource transfer settings for remote node gRPC calls.
/// </summary>
public static class GeoResourceTransferDefaults
{
    /// <summary>
    /// Number of bytes sent in each geo resource transfer chunk.
    /// </summary>
    public const int ChunkSizeBytes = 512 * 1024;
}
