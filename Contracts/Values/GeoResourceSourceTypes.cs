namespace Contracts.Values;

/// <summary>
/// Defines persisted geo resource source type values.
/// </summary>
public static class GeoResourceSourceTypes
{
    /// <summary>
    /// Gets the source type for files managed manually.
    /// </summary>
    public const string Static = "static";

    /// <summary>
    /// Gets the source type for files refreshed from a URL.
    /// </summary>
    public const string AutoUpdate = "autoUpdate";
}

