using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRayne.Cli.Output;
using XRayne.Infrastructure.Services;

namespace XRayne.Cli.Commands.Xray;

public sealed class XrayStartAction(
    ICoreService coreService,
    ICliConsole console,
    ILogger<XrayStartAction> logger)
{
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            await coreService.StartCore(cancellationToken);
            console.Success("xray start completed.");

            return 0;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "xray start failed.");
            console.Error(exception.Message);

            return 1;
        }
    }


    public static Command Create(IServiceProvider provider)
    {

        var timeoutOption = new Option<int>(name: "--timeout")
        {
            Description = "Timeout for call operation",
            DefaultValueFactory = _ => 30
        };

        var command = new Command("start", "Start xray-core process")
        {
            timeoutOption
        };

        command.SetAction(async (parsed, ct) =>
        {
            await using var scope = provider.CreateAsyncScope();
            var action = scope.ServiceProvider.GetRequiredService<XrayStartAction>();

            var timeout = parsed.GetValue(timeoutOption);

            Console.WriteLine(timeout);

            return await action.ExecuteAsync(ct);
        });

        return command;
    }
}
