namespace Whycespace.Shared.Contracts.Engine;

public sealed record WorkflowExecutionResult
{
    public bool IsSuccess { get; init; }
    public object? Output { get; init; }
    public string? FailedStep { get; init; }
    public string? Error { get; init; }
    public IReadOnlyList<object> EmittedEvents { get; init; } = [];

    public static WorkflowExecutionResult Success(object? output = null, IReadOnlyList<object>? events = null) =>
        new() { IsSuccess = true, Output = output, EmittedEvents = events ?? [] };

    public static WorkflowExecutionResult Failure(string failedStep, string error) =>
        new() { IsSuccess = false, FailedStep = failedStep, Error = error };
}
