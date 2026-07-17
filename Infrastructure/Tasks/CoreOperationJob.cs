using Infrastructure.Services;
using Infrastructure.States;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Tasks;

[DisallowConcurrentExecution]
public sealed class CoreOperationJob(
    ICoreService coreService,
    ICoreStateMachine stateMachine,
    ILogger<CoreOperationJob> logger) : IJob
{
    public const string IdentityKey = "id";
    public const string OperationKey = "operation";
    public const string TriggerKeyPrefix = nameof(CoreOperationJob);

    public static readonly JobKey JobKey = new(nameof(CoreOperationJob), "core");

    public async Task Execute(IJobExecutionContext context)
    {
        var operation = Enum.Parse<CoreOperation>(context.MergedJobDataMap.GetString(OperationKey)!);

        stateMachine.DispatchCoreOperationState(CoreOperationState.Running(operation));

        try
        {
            switch (operation)
            {
                case CoreOperation.Start:
                    await coreService.StartAsync(context.CancellationToken);
                    break;
                case CoreOperation.Stop:
                    await coreService.StopAsync(context.CancellationToken);
                    break;
                case CoreOperation.Restart:
                    await coreService.RestartAsync(context.CancellationToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unknown core operation.");
            }

            stateMachine.DispatchCoreOperationState(CoreOperationState.Completed(operation));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Core {Operation} failed.", operation);
            stateMachine.DispatchCoreOperationState(CoreOperationState.Failure(operation, ex.Message));
        }
    }
}
