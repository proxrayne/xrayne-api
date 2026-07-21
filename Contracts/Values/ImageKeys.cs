namespace Contracts.Values;

/// <summary>
/// Formats stable keys for panel-managed images.
/// </summary>
public static class ImageKeys
{
    /// <summary>
    /// Builds a stable application image key from the application identifier.
    /// </summary>
    public static string ForPrefix(int prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Builds a stable application image key from the application identifier.
    /// </summary>
    public static string ForPrefix(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Builds a stable application image key from the application identifier.
    /// </summary>
    public static string New()
    {
        return Guid.NewGuid().ToString("N");
    }
}
