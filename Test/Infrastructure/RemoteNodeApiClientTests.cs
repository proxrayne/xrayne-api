using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Contracts.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RemoteNode;
using RemoteNode.Configurations;
using RemoteNode.Enums;
using RemoteNode.Exceptions;
using RemoteNode.Models;
using RemoteNode.Services;
using Xray.Config.Models;

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
            "start" => await client.StartCoreAsync(CreateStartCoreRequest()),
            "stop" => await client.StopCoreAsync(),
            "restart" => await client.RestartCoreAsync(CreateStartCoreRequest()),
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
            "start" => await client.StartCoreAsync(CreateStartCoreRequest()),
            "restart" => await client.RestartCoreAsync(CreateStartCoreRequest()),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };

        var body = JsonNode.Parse(handler.RequestBody!)!.AsObject();
        var config = body["configTemplate"]!.AsObject();

        config["log"]!["loglevel"]!.GetValue<string>().Should().Be("warning");
        body.ContainsKey("config").Should().BeFalse();
    }

    [Fact]
    public async Task StartCoreAsync_sends_structured_inbound_body()
    {
        var handler = new StubHandler(JsonResponse("""
        {
          "operation": "start",
          "status": "queued"
        }
        """));
        var client = CreateClient(handler);

        await client.StartCoreAsync(CreateStartCoreRequestWithInbound());

        var body = JsonNode.Parse(handler.RequestBody!)!.AsObject();
        var inbounds = body["inbounds"]!.AsArray();
        var item = inbounds.Should().ContainSingle().Subject!.AsObject();

        item["id"]!.GetValue<long>().Should().Be(123);
        item["position"]!.GetValue<int>().Should().Be(0);
        item["inbound"]!["tag"]!.GetValue<string>().Should().Be("vless-in");
    }

    [Fact]
    public async Task Json_endpoints_use_cached_standard_client()
    {
        var handler = new StubHandler(JsonResponse(SamplePingJson()));
        var factory = new StubHttpClientFactory(handler);
        var client = CreateClient(factory);

        await client.PingAsync();

        factory.CreateCount.Should().Be(2);
        factory.Clients[0].Timeout.Should().Be(TimeSpan.FromSeconds(5));
        factory.Clients[1].Timeout.Should().Be(Timeout.InfiniteTimeSpan);
    }

    [Fact]
    public void AddRemoteNodes_registers_api_and_stream_factories()
    {
        var services = new ServiceCollection();
        services.AddRemoteNodes(new RemoteNodeOptions { PingTimeoutSeconds = 5 });

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        var endpoint = new RemoteNodeEndpoint(42, "node.example.test", 8443, "secret");
        provider.GetRequiredService<IRemoteNodeApiClientFactory>().Create(endpoint)
            .Should().BeAssignableTo<IRemoteNodeApiClient>();
        provider.GetRequiredService<IRemoteNodeStreamClientFactory>().Create(endpoint)
            .Should().BeAssignableTo<IRemoteNodeStreamClient>();
    }

    private static IRemoteNodeApiClient CreateClient(HttpResponseMessage response)
        => CreateClient(new StubHandler(response));

    private static IRemoteNodeApiClient CreateClient(StubHandler handler)
    {
        var factory = new StubHttpClientFactory(handler);

        return CreateClient(factory);
    }

    private static IRemoteNodeApiClient CreateClient(StubHttpClientFactory factory)
    {
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

    private static StartCoreRequest CreateStartCoreRequest()
    {
        return new StartCoreRequest
        {
            ConfigTemplate = XrayJsonSerializer.DeserializeRequired<XrayConfig>(
                """{"log":{"loglevel":"warning"}}""",
                "Core config cannot be empty.")
        };
    }

    private static StartCoreRequest CreateStartCoreRequestWithInbound()
    {
        return new StartCoreRequest
        {
            ConfigTemplate = XrayJsonSerializer.DeserializeRequired<XrayConfig>(
                """{"log":{"loglevel":"warning"}}""",
                "Core config cannot be empty."),
            Inbounds =
            [
                new InboundSyncItem
                {
                    Id = 123,
                    Position = 0,
                    Inbound = XrayJsonSerializer.DeserializeRequired<Inbound>(
                        """
                        {
                          "tag": "vless-in",
                          "listen": "0.0.0.0",
                          "port": 443,
                          "protocol": "vless",
                          "settings": {
                            "clients": [
                              { "id": "11111111-1111-1111-1111-111111111111", "email": "alice@example.com" }
                            ],
                            "decryption": "none"
                          }
                        }
                        """,
                        "Inbound config cannot be empty.")
                }
            ]
        };
    }

    private sealed class StubHttpClientFactory(StubHandler handler) : IHttpClientFactory
    {
        public int CreateCount { get; private set; }

        public List<HttpClient> Clients { get; } = [];

        public HttpClient CreateClient(string name)
        {
            CreateCount++;
            var client = new HttpClient(handler);
            Clients.Add(client);

            return client;
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
