namespace Contracts.Configurations;

/// <summary>
/// Configures live remote node log buffers.
/// </summary>
public sealed class NodeLogOptions
{
    /// <summary>
    /// Gets or sets the default number of entries returned by log reads.
    /// </summary>
    public int DefaultLimit { get; set; } = 500;

    /// <summary>
    /// Gets or sets the maximum number of entries retained per node and source.
    /// </summary>
    public int MaxEntriesPerSource { get; set; } = 5000;
}
