namespace Whyce.Shared.Contracts.Runtime;

public sealed record WorkflowStepResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public object? Output { get; init; }
    public IReadOnlyList<object> Events { get; init; } = [];

    public static WorkflowStepResult Success(object? output = null, IReadOnlyList<object>? events = null) =>
        new() { IsSuccess = true, Output = output, Events = events ?? [] };

    public static WorkflowStepResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
