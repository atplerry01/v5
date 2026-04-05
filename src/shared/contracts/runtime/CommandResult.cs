namespace Whyce.Shared.Contracts.Runtime;

public sealed record CommandResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public IReadOnlyList<object> EmittedEvents { get; init; } = [];

    public static CommandResult Success(IReadOnlyList<object> events) =>
        new() { IsSuccess = true, EmittedEvents = events };

    public static CommandResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
