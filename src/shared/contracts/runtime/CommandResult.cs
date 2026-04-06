namespace Whyce.Shared.Contracts.Runtime;

public sealed record CommandResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public object? Output { get; init; }
    public IReadOnlyList<object> EmittedEvents { get; init; } = [];
    public bool EventsRequirePersistence { get; init; }

    public static CommandResult Success(IReadOnlyList<object> events, object? output = null, bool eventsRequirePersistence = true) =>
        new() { IsSuccess = true, EmittedEvents = events, Output = output, EventsRequirePersistence = eventsRequirePersistence };

    public static CommandResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
