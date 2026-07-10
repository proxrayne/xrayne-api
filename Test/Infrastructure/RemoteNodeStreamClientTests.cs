using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Enums;
using RemoteNode.Models;
using RemoteNode.Services;

namespace Test.Infrastructure;

public sealed class RemoteNodeStreamClientTests
{
    [Fact]
    public async Task ConnectStreamAsync_reads_server_sent_events()
    {
        var payload = SampleConnectionEventJson();
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"data: {payload}\n\n", Encoding.UTF8, "text/event-stream")
        });

        var events = new List<NodeConnectionEvent>();
        await foreach (var item in client.ConnectStreamAsync())
        {
            events.Add(item);
        }

        events.Should().ContainSingle();
        events[0].Type.Should().Be("heartbeat");
        events[0].Timestamp.Should().Be(DateTimeOffset.Parse("2026-07-03T12:00:00+00:00"));
        events[0].Ping?.NodeVersion.Should().Be("1.2.3");
    }

    [Fact]
    public async Task ConnectStreamAsync_reads_runtime_event_payloads()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "data: {\"type\":\"core_status\",\"timestamp\":\"2026-07-03T12:00:00+00:00\",\"core\":{\"isInstalled\":true,\"status\":\"started\",\"isInstalling\":false,\"version\":\"25.7.1\"}}\n\n"
                + "data: {\"type\":\"core_install\",\"timestamp\":\"2026-07-03T12:00:01+00:00\",\"install\":{\"jobId\":\"job-1\",\"step\":\"installed\",\"message\":\"done\",\"updatedAt\":\"2026-07-03T12:00:01+00:00\"}}\n\n",
                Encoding.UTF8,
                "text/event-stream")
        });

        var events = new List<NodeConnectionEvent>();
        await foreach (var item in client.ConnectStreamAsync())
        {
            events.Add(item);
        }

        events.Should().HaveCount(2);
        events[0].Type.Should().Be("core_status");
        events[0].Core?.Status.Should().Be(RemoteCoreStatus.Started);
        events[0].Core?.Version.Should().Be("25.7.1");
        events[1].Type.Should().Be("core_install");
        events[1].Install?.JobId.Should().Be("job-1");
        events[1].Install?.Step.Should().Be(InstallCoreStep.Installed);
    }

    [Fact]
    public async Task LogStreamAsync_reads_server_sent_events()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "data: {\"type\":\"entry\",\"entry\":{\"id\":\"log-1\",\"timestamp\":\"2026-07-03T12:00:00+00:00\",\"level\":\"info\",\"message\":\"ready\"}}\n\n",
                Encoding.UTF8,
                "text/event-stream")
        });

        var events = new List<RemoteLogStreamEvent>();
        await foreach (var item in client.LogStreamAsync(10))
        {
            events.Add(item);
        }

        events.Should().ContainSingle();
        events[0].Entry!.Message.Should().Be("ready");
    }

    [Fact]
    public async Task CoreStatusStreamAsync_reads_server_sent_events()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "data: {\"isInstalled\":true,\"status\":\"stopped\",\"isInstalling\":false,\"version\":\"25.7.1\"}\n\n",
                Encoding.UTF8,
                "text/event-stream")
        });

        var events = new List<CoreStatusResponse>();
        await foreach (var item in client.CoreStatusStreamAsync())
        {
            events.Add(item);
        }

        events.Should().ContainSingle();
        events[0].Status.Should().Be(RemoteCoreStatus.Stopped);
    }

    [Fact]
    public async Task InstallCoreStatusStreamAsync_reads_server_sent_events()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "data: {\"jobId\":\"job-1\",\"step\":\"installed\",\"message\":\"done\",\"updatedAt\":\"2026-07-04T12:00:00+00:00\"}\n\n",
                Encoding.UTF8,
                "text/event-stream")
        });

        var events = new List<InstallCoreStatusResponse>();
        await foreach (var item in client.InstallCoreStatusStreamAsync("job-1"))
        {
            events.Add(item);
        }

        events.Should().ContainSingle();
        events[0].JobId.Should().Be("job-1");
        events[0].Step.Should().Be(InstallCoreStep.Installed);
    }

    [Fact]
    public async Task Stream_requests_use_event_stream_accept_header_and_infinite_timeout()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($"data: {SampleConnectionEventJson()}\n\n", Encoding.UTF8, "text/event-stream")
        });
        var factory = new StubHttpClientFactory(handler);
        var client = CreateClient(factory);

        await foreach (var _ in client.ConnectStreamAsync())
        {
        }

        handler.AcceptHeaders.Should().Contain("text/event-stream");
        factory.Clients[0].Timeout.Should().Be(TimeSpan.FromSeconds(5));
        factory.Clients[1].Timeout.Should().Be(Timeout.InfiniteTimeSpan);
    }

    private static IRemoteNodeStreamClient CreateClient(HttpResponseMessage response)
        => CreateClient(new StubHttpClientFactory(new StubHandler(response)));

    private static IRemoteNodeStreamClient CreateClient(StubHttpClientFactory factory)
    {
        var options = Options.Create(new RemoteNodeOptions { PingTimeoutSeconds = 5 });
        var endpoint = new RemoteNodeEndpoint(42, "node.example.test", 8443, "secret");

        return new RemoteNodeStreamClient(factory, options, endpoint);
    }

    private static string SampleConnectionEventJson()
    {
        return """
        {"type":"heartbeat","timestamp":"2026-07-03T12:00:00+00:00","ping":{"nodeVersion":"1.2.3","environment":"Development","uptime":"01:00:00","core":{"isInstalled":true,"isRunning":true,"version":"24.9.30","status":"started"}}}
        """;
    }

    private sealed class StubHttpClientFactory(StubHandler handler) : IHttpClientFactory
    {
        public List<HttpClient> Clients { get; } = [];

        public HttpClient CreateClient(string name)
        {
            var client = new HttpClient(handler);
            Clients.Add(client);

            return client;
        }
    }

    private sealed class StubHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public List<string> AcceptHeaders { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            AcceptHeaders.AddRange(request.Headers.Accept.Select(header => header.MediaType ?? string.Empty));

            return Task.FromResult(response);
        }
    }
}
