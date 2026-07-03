using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Exceptions;
using RemoteNode.Models;
using RemoteNode.Services;

namespace Test.Infrastructure;

public sealed class RemoteNodeApiClientTests
{
    [Fact]
    public async Task PingAsync_returns_remote_node_telemetry()
    {
        var client = CreateClient(JsonResponse(SamplePingJson()));

        var ping = await client.PingAsync();

        ping.Service.Should().Be("xrayne-node");
        ping.NodeVersion.Should().Be("1.2.3");
        ping.Core.Version.Should().Be("24.9.30");
        ping.System.MachineName.Should().Be("node-a");
    }

    [Fact]
    public async Task PingAsync_maps_non_success_response()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("upstream failed")
        });

        var exception = await Assert.ThrowsAsync<RemoteNodeHttpException>(() => client.PingAsync());

        exception.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        exception.ResponseBody.Should().Be("upstream failed");
    }

    [Fact]
    public async Task PingAsync_maps_invalid_json_response()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{", Encoding.UTF8, "application/json")
        });

        await Assert.ThrowsAsync<RemoteNodeProtocolException>(() => client.PingAsync());
    }

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
        events[0].Ping?.System.MachineName.Should().Be("node-a");
    }

    private static IRemoteNodeApiClient CreateClient(HttpResponseMessage response)
    {
        var factory = new StubHttpClientFactory(response);
        var options = Options.Create(new RemoteNodeOptions { PingTimeoutSeconds = 5 });
        var endpoint = new RemoteNodeEndpoint(42, "node.example.test", 8443, "secret");

        return new RemoteNodeApiClient(factory, options, endpoint);
    }

    private static HttpResponseMessage JsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private static string SampleConnectionEventJson()
        => $$"""{"type":"heartbeat","timestamp":"2026-07-03T12:00:00+00:00","ping":{{SamplePingJson()}}}""";

    private static string SamplePingJson()
    {
        return """
        {
          "service": "xrayne-node",
          "nodeVersion": "1.2.3",
          "environment": "Development",
          "startedAt": "2026-07-03T11:00:00+00:00",
          "timestamp": "2026-07-03T12:00:00+00:00",
          "uptime": "01:00:00",
          "core": {
            "isInstalled": true,
            "isRunning": true,
            "version": "24.9.30",
            "status": "running"
          },
          "system": {
            "machineName": "node-a",
            "osDescription": "Linux",
            "processorCount": 2,
            "workingSetBytes": 1024,
            "gcTotalMemoryBytes": 512,
            "currentProcessThreadCount": 4,
            "systemThreadCount": 32,
            "startedAt": "2026-07-03T11:00:00+00:00",
            "timestamp": "2026-07-03T12:00:00+00:00",
            "uptime": "01:00:00",
            "cpu": {
              "logicalCoreCount": 2,
              "averageUsagePercent": 12.5,
              "cores": [
                { "index": 0, "usagePercent": 10.0 },
                { "index": 1, "usagePercent": 15.0 }
              ]
            },
            "memory": {
              "totalBytes": 4096,
              "usedBytes": 2048,
              "availableBytes": 2048
            },
            "swap": {
              "totalBytes": 0,
              "usedBytes": 0,
              "availableBytes": 0
            },
            "volumes": [],
            "network": {
              "ipv4Addresses": ["10.0.0.1"],
              "ipv6Addresses": []
            }
          }
        }
        """;
    }

    private sealed class StubHttpClientFactory(HttpResponseMessage response) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(new StubHandler(response));
        }
    }

    private sealed class StubHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(response);
        }
    }
}
