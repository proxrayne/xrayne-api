using System.Net;
using System.Net.Http.Json;
using Api.Responses;
using Contracts.Enums;
using Test.Infrastructure;

namespace Test.Integration;

/// <summary>
/// Tests random name API endpoints.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class RandomNamesApiTests
{
    private readonly PostgresFixture _postgres;

    public RandomNamesApiTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Generate_WithoutToken_Returns401()
    {
        await using var factory = await CreateFactoryAsync();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/random-names/generate");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Generate_WithAnyAuthenticatedAdmin_ReturnsGeneratedName()
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminPermission.ViewLogs);

        var response = await client.GetAsync("/api/random-names/generate");
        var body = await response.Content.ReadFromJsonAsync<RandomNameResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body!.Name.Should().NotBeNullOrWhiteSpace();
        body.Name.Should().MatchRegex(@"^[a-z]+-[a-z]+-\d{4}$");
    }

    private async Task<XRayneWebApplicationFactory> CreateFactoryAsync()
    {
        await _postgres.ResetAsync();

        return new XRayneWebApplicationFactory(_postgres.ConnectionString);
    }
}
