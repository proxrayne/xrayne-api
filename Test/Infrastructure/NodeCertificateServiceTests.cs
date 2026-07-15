using System.Net;
using Data.Contracts;
using Data.Entities;
using Infrastructure.Services;
using Node.Exceptions;
using Node.Models;
using Node.Services;

namespace Test.Infrastructure;

public sealed class NodeCertificateServiceTests
{
    [Fact]
    public async Task IssueAsync_stores_only_certificate_metadata()
    {
        var fixture = NodeCertificateServiceFixture.Create();
        CertificateEntity? saved = null;
        fixture.Repository
            .GetByDomainAsync(fixture.AdminId, fixture.Node.Id, "example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<CertificateEntity?>(null));
        fixture.Repository
            .AddAsync(Arg.Any<CertificateEntity>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                saved = call.Arg<CertificateEntity>();
                saved.Id = 10;
                return Task.FromResult(saved);
            });
        fixture.NodeClient
            .IssueCertificateAsync(Arg.Any<IssueCertificateRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CreateRemoteCertificate()));

        var result = await fixture.Service.IssueAsync(
            fixture.AdminId,
            fixture.Node,
            new IssueCertificateRequest("Example.COM"),
            CancellationToken.None);

        result.Id.Should().Be(10);
        saved.Should().NotBeNull();
        saved!.Domain.Should().Be("example.com");
        saved.Active.Should().BeTrue();
        saved.CertificateFile.Should().Be("/srv/xrayne/certificates/example.com/fullchain.pem");
        saved.PrivateKeyFile.Should().Be("/srv/xrayne/certificates/example.com/privkey.pem");
        saved.Node.Should().BeSameAs(fixture.Node);
        saved.Admin.Should().BeSameAs(fixture.Node.Admin);
    }

    [Fact]
    public async Task DeleteAsync_removes_local_metadata_when_remote_certificate_is_missing()
    {
        var fixture = NodeCertificateServiceFixture.Create();
        fixture.NodeClient
            .DeleteCertificateAsync("example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new NodeHttpException(
                fixture.Node.Id,
                "api/certificates/example.com",
                HttpStatusCode.NotFound,
                null)));

        await fixture.Service.DeleteAsync(fixture.AdminId, fixture.Node, "Example.COM", CancellationToken.None);

        await fixture.Repository
            .Received(1)
            .DeleteByDomainAsync(fixture.AdminId, fixture.Node.Id, "example.com", Arg.Any<CancellationToken>());
    }

    private static CertificateDto CreateRemoteCertificate()
    {
        return new CertificateDto(
            "example.com",
            true,
            DateTimeOffset.UtcNow.AddDays(30),
            "/srv/xrayne/certificates/example.com/fullchain.pem",
            "/srv/xrayne/certificates/example.com/privkey.pem");
    }

    private sealed class NodeCertificateServiceFixture
    {
        private NodeCertificateServiceFixture(
            Guid adminId,
            NodeEntity node,
            ICertificateRepository repository,
            INodeCertificateClient nodeClient,
            NodeCertificateService service)
        {
            AdminId = adminId;
            Node = node;
            Repository = repository;
            NodeClient = nodeClient;
            Service = service;
        }

        public Guid AdminId { get; }

        public NodeEntity Node { get; }

        public ICertificateRepository Repository { get; }

        public INodeCertificateClient NodeClient { get; }

        public NodeCertificateService Service { get; }

        public static NodeCertificateServiceFixture Create()
        {
            var adminId = Guid.NewGuid();
            var node = new NodeEntity
            {
                Id = 7,
                Name = "node",
                Address = "node.example.com",
                Port = 22,
                ApiPort = 8443,
                SSHUsername = "root",
                EncryptedApiKey = "encrypted",
                ApiKeyFingerprint = "fingerprint",
                WorkingDirectory = "/srv/xrayne",
                Admin = new AdminAccount
                {
                    Id = adminId,
                    Username = "admin",
                    PasswordHash = "hash"
                }
            };
            var repository = Substitute.For<ICertificateRepository>();
            var secrets = Substitute.For<INodeSecretService>();
            var factory = Substitute.For<INodeCertificateClientFactory>();
            var nodeClient = Substitute.For<INodeCertificateClient>();
            secrets.UnprotectApiKey("encrypted").Returns("api-key");
            factory.Create(Arg.Any<NodeEndpoint>()).Returns(nodeClient);
            var service = new NodeCertificateService(repository, secrets, factory);

            return new NodeCertificateServiceFixture(adminId, node, repository, nodeClient, service);
        }
    }
}
