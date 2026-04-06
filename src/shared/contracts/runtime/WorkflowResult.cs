namespace Whyce.Shared.Contracts.Runtime;

public sealed record WorkflowResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public object? Output { get; init; }
    public IReadOnlyList<object> EmittedEvents { get; init; } = [];

    public static WorkflowResult Success(IReadOnlyList<object>? events = null, object? output = null) =>
        new() { IsSuccess = true, EmittedEvents = events ?? [], Output = output };

    public static WorkflowResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
