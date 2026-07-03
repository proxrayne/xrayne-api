namespace Api.Responses;

public sealed record CoreReleasesQuery(
    int PerPage = 10,
    int Page = 1
);