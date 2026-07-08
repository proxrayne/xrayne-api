using System.Net;
using System.Text;
using Contracts.Utilities;
using Contracts.Values;
using Data.Contracts;
using Data.Entities;
using Infrastructure.Services;
using RemoteNode.Models;
using RemoteNode.Services;
using Xray.Config.Models;

namespace Test.Infrastructure;

public sealed class NodeGeoResourceServiceTests
{
    [Fact]
    public async Task SynchronizeNodeAsync_adds_unknown_remote_files_as_static()
    {
        var fixture = NodeGeoResourceServiceFixture.Create();
        GeoResourceEntity? saved = null;
        fixture.Repository
            .GetAllAsync(fixture.AdminId, fixture.Node.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<GeoResourceEntity>()));
        fixture.Repository
            .GetByFilenameAsync(fixture.AdminId, fixture.Node.Id, "geoip.dat", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GeoResourceEntity?>(null));
        fixture.Repository
            .AddAsync(Arg.Any<GeoResourceEntity>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                saved = call.Arg<GeoResourceEntity>();
                saved.Id = 10;

                return Task.FromResult(saved);
            });
        fixture.RemoteClient
            .GetGeoResourcesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<GeoResourceDto>
            {
                new("geoip.dat", 128, DateTimeOffset.UtcNow)
            }));

        await fixture.Service.SynchronizeNodeAsync(fixture.AdminId, fixture.Node);

        saved.Should().NotBeNull();
        saved!.Filename.Should().Be("geoip.dat");
        saved.SourceType.Should().Be(GeoResourceSourceTypes.Static);
        saved.Node.Should().Be(fixture.Node);
        saved.Admin.Should().Be(fixture.Node.Admin);
    }

    [Fact]
    public async Task RefreshDueAutoUpdatesAsync_keeps_next_run_when_download_fails()
    {
        var fixture = NodeGeoResourceServiceFixture.Create(HttpStatusCode.InternalServerError);
        var nextRunAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        var resource = new GeoResourceEntity
        {
            Id = 10,
            Filename = "geosite.dat",
            SizeBytes = 10,
            LastModifiedAt = DateTimeOffset.UtcNow.AddHours(-1),
            SourceType = GeoResourceSourceTypes.AutoUpdate,
            Url = "https://example.com/geosite.dat",
            CronTemplate = "*/10 * * * *",
            NextRunAt = nextRunAt,
            Node = fixture.Node,
            Admin = fixture.Node.Admin
        };
        GeoResourceEntity? updated = null;
        fixture.Repository
            .GetDueAutoUpdateAsync(Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<GeoResourceEntity> { resource }));
        fixture.Repository
            .UpdateAsync(Arg.Any<GeoResourceEntity>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                updated = call.Arg<GeoResourceEntity>();
                return Task.FromResult<GeoResourceEntity?>(updated);
            });

        await fixture.Service.RefreshDueAutoUpdatesAsync();

        updated.Should().NotBeNull();
        updated!.LastError.Should().NotBeNullOrWhiteSpace();
        updated.LastErrorAt.Should().NotBeNull();
        updated.NextRunAt.Should().Be(nextRunAt);
        await fixture.RemoteClient.DidNotReceiveWithAnyArgs()
            .UploadGeoResourceAsync(default!, default!, default);
    }

    [Fact]
    public async Task UpdateAsync_preserves_next_run_when_auto_resource_is_renamed_only()
    {
        var fixture = NodeGeoResourceServiceFixture.Create();
        var nextRunAt = DateTimeOffset.UtcNow.AddHours(1);
        var resource = new GeoResourceEntity
        {
            Id = 10,
            Filename = "geosite.dat",
            SizeBytes = 10,
            LastModifiedAt = DateTimeOffset.UtcNow.AddHours(-1),
            SourceType = GeoResourceSourceTypes.AutoUpdate,
            Url = "https://example.com/geosite.dat",
            CronTemplate = "*/10 * * * *",
            NextRunAt = nextRunAt,
            Node = fixture.Node,
            Admin = fixture.Node.Admin
        };
        GeoResourceEntity? updated = null;
        fixture.Repository
            .GetByIdAsync(fixture.AdminId, fixture.Node.Id, resource.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GeoResourceEntity?>(resource));
        fixture.Repository
            .GetByFilenameAsync(fixture.AdminId, fixture.Node.Id, "custom.dat", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GeoResourceEntity?>(null));
        fixture.RemoteClient
            .RenameGeoResourceAsync(
                "geosite.dat",
                Arg.Is<RenameGeoResourceRequest>(request => request.FileName == "custom.dat"),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new GeoResourceDto("custom.dat", 10, resource.LastModifiedAt)));
        fixture.Repository
            .UpdateAsync(fixture.AdminId, Arg.Any<GeoResourceEntity>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                updated = call.Arg<GeoResourceEntity>();
                return Task.FromResult<GeoResourceEntity?>(updated);
            });

        await fixture.Service.UpdateAsync(
            fixture.AdminId,
            fixture.Node,
            resource.Id,
            "custom.dat",
            resource.Url,
            resource.CronTemplate);

        updated.Should().NotBeNull();
        updated!.Filename.Should().Be("custom.dat");
        updated.NextRunAt.Should().Be(nextRunAt);
        await fixture.RemoteClient.DidNotReceiveWithAnyArgs()
            .UploadGeoResourceAsync(default!, default!, default);
    }

    private sealed class NodeGeoResourceServiceFixture
    {
        private NodeGeoResourceServiceFixture(
            Guid adminId,
            NodeEntity node,
            IGeoResourceRepository repository,
            IRemoteNodeApiClient remoteClient,
            NodeGeoResourceService service)
        {
            AdminId = adminId;
            Node = node;
            Repository = repository;
            RemoteClient = remoteClient;
            Service = service;
        }

        public Guid AdminId { get; }

        public NodeEntity Node { get; }

        public IGeoResourceRepository Repository { get; }

        public IRemoteNodeApiClient RemoteClient { get; }

        public NodeGeoResourceService Service { get; }

        public static NodeGeoResourceServiceFixture Create(HttpStatusCode downloadStatusCode = HttpStatusCode.OK)
        {
            var adminId = Guid.NewGuid();
            var admin = new AdminAccount
            {
                Id = adminId,
                Username = "admin",
                PasswordHash = "hash"
            };
            var node = new NodeEntity
            {
                Id = 7,
                Name = "node",
                Address = "node.example",
                ApiPort = 9443,
                Port = 22,
                SSHUsername = "root",
                WorkingDirectory = "/opt/xrayne-node",
                EncryptedApiKey = "protected",
                ApiKeyFingerprint = "fingerprint",
                LastStatusChange = DateTime.UtcNow,
                Admin = admin
            };
            var repository = Substitute.For<IGeoResourceRepository>();
            var remoteClient = Substitute.For<IRemoteNodeApiClient>();
            var factory = Substitute.For<IRemoteNodeApiClientFactory>();
            factory.Create(Arg.Any<RemoteNodeEndpoint>()).Returns(remoteClient);

            var secrets = Substitute.For<INodeSecretService>();
            secrets.UnprotectApiKey("protected").Returns("api-key");
            var coreStates = Substitute.For<IRemoteNodeCoreStateStore>();
            var builder = Substitute.For<INodeCoreConfigBuilder>();
            builder.Build(node).Returns(new XrayConfig());
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(new StubHandler(downloadStatusCode)));

            var service = new NodeGeoResourceService(
                repository,
                secrets,
                factory,
                coreStates,
                builder,
                httpClientFactory);

            return new NodeGeoResourceServiceFixture(adminId, node, repository, remoteClient, service);
        }
    }

    private sealed class StubHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes("geo"))
            });
        }
    }
}
