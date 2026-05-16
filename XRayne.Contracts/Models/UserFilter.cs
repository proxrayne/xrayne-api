using Xray.Config.Enums;
using XRayne.Contracts.Enums;

namespace XRayne.Contracts.Models;

public sealed record UserFilter : CursorQuery
{
    public IReadOnlyCollection<Protocol>? Protocol { get; init; }

    public IReadOnlyCollection<UserStatus>? Status { get; init; }

    public IReadOnlyCollection<LimitResetStrategy>? LimitResetStrategy { get; init; }
}
