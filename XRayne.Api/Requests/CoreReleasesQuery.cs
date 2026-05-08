namespace XRayne.Api.Responses;

public sealed record CoreReleasesQuery(
    int PerPage = 10,
    int Page = 1,
    bool NoCache = false
);