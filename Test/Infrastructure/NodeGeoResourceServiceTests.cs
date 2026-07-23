using Data.Contracts;
using Data.Entities;
using Infrastructure.Services;
using Node.Models;
using Node.Services;

namespace Test.Infrastructure;

/// <summary>
/// Tests node geo resource synchronization behavior.
/// </summary>
public sealed class NodeGeoResourceServiceTests
{
    [Fact]
    public async Task SynchronizeNodeAsync_AddsRemoteResourceWithoutDetachedNavigationGraph()
    {
        var geoResources = Substitute.For<IGeoResourceRepository>();
        var secrets = Substitute.For<INodeSecretService>();
        var geoResourceClientFactory = Substitute.For<INodeGeoResourceClientFactory>();
        var geoResourceClient = Substitute.For<INodeGeoResourceClient>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var scheduler = Substitute.For<IBackgroundTaskScheduler>();
        var coreService = Substitute.For<INodeCoreService>();
        var fileStorage = Substitute.For<ITempFileStorage>();
        var lastModifiedAt = DateTimeOffset.UtcNow;
        var node = new NodeEntity
        {
            Id = 42,
            Name = "node-1",
            Address = "127.0.0.1",
            ApiPort = 62050,
            EncryptedApiKey = "protected-key",
            ApiKeyFingerprint = "fingerprint",
            AdminId = 7,
            Admin = new AdminAccountEntity
            {
                Id = 7,
                Username = "admin",
                PasswordHash = "hash"
            }
        };
        GeoResourceEntity? created = null;

        secrets.UnprotectApiKey("protected-key").Returns("plain-key");
        geoResourceClientFactory
            .Create(Arg.Is<NodeEndpoint>(endpoint =>
                endpoint.NodeId == node.Id &&
                endpoint.Address == node.Address &&
                endpoint.ApiPort == node.ApiPort &&
                endpoint.ApiKey == "plain-key"))
            .Returns(geoResourceClient);
        geoResourceClient
            .GetGeoResourcesAsync(Arg.Any<CancellationToken>())
            .Returns([new GeoResourceDto("geoip.dat", 128, lastModifiedAt)]);
        geoResources
            .GetAllAsync(node.AdminId, node.Id, Arg.Any<CancellationToken>())
            .Returns([]);
        geoResources
            .GetByFilenameAsync(node.AdminId, node.Id, "geoip.dat", Arg.Any<CancellationToken>())
            .Returns((GeoResourceEntity?)null);
        geoResources
            .AddAsync(Arg.Do<GeoResourceEntity>(entity => created = entity), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<GeoResourceEntity>());

        var service = new NodeGeoResourceService(
            geoResources,
            secrets,
            geoResourceClientFactory,
            httpClientFactory,
            scheduler,
            coreService,
            fileStorage);

        await service.SynchronizeNodeAsync(node, CancellationToken.None);

        created.Should().NotBeNull();
        created!.NodeId.Should().Be(node.Id);
        created.AdminId.Should().Be(node.AdminId);
        created.Node.Should().BeNull();
        created.Admin.Should().BeNull();
    }
}
