using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Enums;
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

        ping.NodeVersion.Should().Be("1.2.3");
        ping.Environment.Should().Be("Development");
        ping.Core.Version.Should().Be("24.9.30");
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
    public async Task GetSystemStatusAsync_returns_remote_system_status()
    {
        var client = CreateClient(JsonResponse(SampleSystemStatusJson()));

        var status = await client.GetSystemStatusAsync();

        status.Timestamp.Should().Be(DateTimeOffset.Parse("2026-07-03T12:00:00+00:00"));
        status.System.MachineName.Should().Be("node-a");
        status.System.Cpu.LogicalCoreCount.Should().Be(2);
    }

    [Fact]
    public async Task GetCoreStatusAsync_returns_remote_core_status()
    {
        var client = CreateClient(JsonResponse("""
        {
          "isInstalled": true,
          "status": "started",
          "isInstalling": false,
          "version": "25.7.1",
          "startedAt": "2026-07-03T11:55:00+00:00",
          "uptime": "00:05:00"
        }
        """));

        var status = await client.GetCoreStatusAsync();

        status.IsInstalled.Should().BeTrue();
        status.Status.Should().Be(RemoteCoreStatus.Started);
        status.IsInstalling.Should().BeFalse();
        status.Version.Should().Be("25.7.1");
        status.StartedAt.Should().Be(DateTimeOffset.Parse("2026-07-03T11:55:00+00:00"));
        status.Uptime.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task InstallCoreAsync_returns_remote_install_job()
    {
        var client = CreateClient(JsonResponse("""
        {
          "jobId": "job-1",
          "version": "latest",
          "status": "queued"
        }
        """));

        var result = await client.InstallCoreAsync(new InstallCoreRequest("latest"));

        result.JobId.Should().Be("job-1");
        result.Version.Should().Be("latest");
        result.Status.Should().Be("queued");
    }

    [Fact]
    public async Task GetInstallCoreStatusAsync_returns_remote_install_status()
    {
        var client = CreateClient(JsonResponse("""
        {
          "jobId": "job-1",
          "step": "installed",
          "message": "done",
          "updatedAt": "2026-07-04T12:00:00+00:00"
        }
        """));

        var result = await client.GetInstallCoreStatusAsync("job-1");

        result.JobId.Should().Be("job-1");
        result.Step.Should().Be(InstallCoreStep.Installed);
        result.Message.Should().Be("done");
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

    [Theory]
    [InlineData("start")]
    [InlineData("stop")]
    [InlineData("restart")]
    [InlineData("runtime-restart")]
    public async Task Core_operation_methods_return_accepted_operation(string operation)
    {
        var responseOperation = operation == "runtime-restart" ? "restart" : operation;
        var client = CreateClient(JsonResponse($$"""
        {
          "operation": "{{responseOperation}}",
          "status": "queued"
        }
        """));

        var result = operation switch
        {
            "start" => await client.StartCoreAsync(new StartCoreRequest(CreateCoreConfig())),
            "stop" => await client.StopCoreAsync(),
            "restart" => await client.RestartCoreAsync(new StartCoreRequest(CreateCoreConfig())),
            "runtime-restart" => await client.RestartRuntimeAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };

        result.Operation.Should().Be(responseOperation);
        result.Status.Should().Be("queued");
    }

    [Theory]
    [InlineData("start")]
    [InlineData("restart")]
    public async Task Core_config_operations_send_config_body(string operation)
    {
        var response = JsonResponse($$"""
        {
          "operation": "{{operation}}",
          "status": "queued"
        }
        """);
        var handler = new StubHandler(response);
        var client = CreateClient(handler);

        _ = operation switch
        {
            "start" => await client.StartCoreAsync(new StartCoreRequest(CreateCoreConfig())),
            "restart" => await client.RestartCoreAsync(new StartCoreRequest(CreateCoreConfig())),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };

        var body = JsonNode.Parse(handler.RequestBody!)!.AsObject();
        var config = JsonNode.Parse(body["config"]!.GetValue<string>())!.AsObject();

        config["log"]!["loglevel"]!.GetValue<string>().Should().Be("warning");
    }

    private static IRemoteNodeApiClient CreateClient(HttpResponseMessage response)
        => CreateClient(new StubHandler(response));

    private static IRemoteNodeApiClient CreateClient(StubHandler handler)
    {
        var factory = new StubHttpClientFactory(handler);
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
    {
        return """
        {"type":"heartbeat","timestamp":"2026-07-03T12:00:00+00:00","ping":{"nodeVersion":"1.2.3","environment":"Development","uptime":"01:00:00","core":{"isInstalled":true,"isRunning":true,"version":"24.9.30","status":"started"}}}
        """;
    }

    private static string SamplePingJson()
    {
        return """
        {
          "nodeVersion": "1.2.3",
          "environment": "Development",
          "uptime": "01:00:00",
          "core": {
            "isInstalled": true,
            "isRunning": true,
            "version": "24.9.30",
            "status": "started"
          }
        }
        """;
    }

    private static string SampleSystemStatusJson()
    {
        return """
        {
          "timestamp": "2026-07-03T12:00:00+00:00",
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

    private static string CreateCoreConfig() => """{"log":{"loglevel":"warning"}}""";

    private sealed class StubHttpClientFactory(StubHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(handler);
        }
    }

    private sealed class StubHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public string? RequestBody { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestBody = request.Content?.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();

            return Task.FromResult(response);
        }
    }
}
