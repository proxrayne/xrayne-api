namespace Api.Responses;

/// <summary>
/// Response containing panel bootstrap settings and pending restart state.
/// </summary>
public sealed class PanelSettingsResponse
{
    /// <summary>
    /// Gets panel bootstrap settings visible through the API.
    /// </summary>
    public required PanelSettingsDto Settings { get; init; }

    /// <summary>
    /// Gets or sets whether a panel restart is pending.
    /// </summary>
    public bool PendingRestart { get; set; }
}
