namespace Infrastructure.States;

public sealed class CoreOperationState
{
    public required CoreOperation Operation { get; init; }

    public required CoreOperationStep Step { get; init; }

    public string? Message { get; init; }

    public bool IsActive => Step is CoreOperationStep.Queued or CoreOperationStep.Running;

    public static CoreOperationState Queued(CoreOperation operation) => new()
    {
        Operation = operation,
        Step = CoreOperationStep.Queued,
        Message = $"Core {operation.ToString().ToLowerInvariant()} scheduled.",
    };

    public static CoreOperationState Running(CoreOperation operation) => new()
    {
        Operation = operation,
        Step = CoreOperationStep.Running,
        Message = $"Core {operation.ToString().ToLowerInvariant()} started.",
    };

    public static CoreOperationState Completed(CoreOperation operation) => new()
    {
        Operation = operation,
        Step = CoreOperationStep.Completed,
        Message = $"Core {operation.ToString().ToLowerInvariant()} completed.",
    };

    public static CoreOperationState Failure(CoreOperation operation, string message) => new()
    {
        Operation = operation,
        Step = CoreOperationStep.Failure,
        Message = message,
    };
}
