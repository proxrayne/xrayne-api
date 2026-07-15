using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Contracts.Configurations;

namespace Infrastructure.Services;

/// <summary>
/// Starts and periodically reconciles live remote node connections.
/// </summary>
public sealed class NodeConnectionHostedService(
    INodeConnectionManager connectionManager,
    IOptions<NodeConnectionOptions> connectionOptions,
    ILogger<NodeConnectionHostedService> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Remote node connection supervisor started.");

        await StartAllAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await DelaySupervisorAsync(stoppingToken);
            await StartAllAsync(stoppingToken);
        }
    }

    private async Task StartAllAsync(CancellationToken stoppingToken)
    {
        try
        {
            await connectionManager.StartAllAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        catch (Exception exception)
        {
            logger.LogError(exception, "Remote node connection supervisor synchronization failed.");
        }
    }

    private async Task DelaySupervisorAsync(CancellationToken stoppingToken)
    {
        var seconds = Math.Clamp(connectionOptions.Value.SupervisorIntervalSeconds, 30, 300);
        await Task.Delay(TimeSpan.FromSeconds(seconds), stoppingToken);
    }
}
