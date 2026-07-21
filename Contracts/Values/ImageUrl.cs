namespace Contracts.Values;

/// <summary>
/// Formats public image URLs used by browser clients.
/// </summary>
public static class ImageUrl
{
    /// <summary>
    /// Builds a versioned image URL for the specified image key.
    /// </summary>
    public static string Build(string key, long version) => $"/image/{Uri.EscapeDataString(key)}?v={version}";
}
