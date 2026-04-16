namespace Whycespace.Shared.Contracts.Runtime;

public sealed record WorkflowResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public object? Output { get; init; }
    public IReadOnlyList<object> EmittedEvents { get; init; } = [];

    // Closed-loop correlation: mirrors CommandResult.CorrelationId so workflow
    // entry points (RevenueController, etc.) can echo the runtime-stamped id
    // back to the API caller.
    public Guid CorrelationId { get; init; }

    public static WorkflowResult Success(IReadOnlyList<object>? events = null, object? output = null) =>
        new() { IsSuccess = true, EmittedEvents = events ?? [], Output = output };

    public static WorkflowResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
