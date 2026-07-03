namespace Api.Responses;

public sealed class UpdatePanelSettingsResponse
{
    public bool RequiresRestart { get; set; }

    public List<string> ChangedFields { get; set; } = [];
}
