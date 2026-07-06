using Contracts.Enums;

namespace Infrastructure.States;

public sealed record CoreState(
    bool IsInstalled,
    CoreStatus? Status,
    bool IsInstalling,
    string? Version);
