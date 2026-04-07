namespace Whyce.Shared.Contracts.Runtime;

public sealed record CommandResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public object? Output { get; init; }
    public IReadOnlyList<object> EmittedEvents { get; init; } = [];
    public bool EventsRequirePersistence { get; init; }

    /// <summary>
    /// Optional audit emission carried alongside the result. Persisted by the
    /// runtime control plane to a dedicated stream BEFORE domain events,
    /// independent of IsSuccess. Used by PolicyMiddleware to record allow/deny
    /// decisions to the constitutional-system policy decision stream.
    /// PolicyDecisionHash on the context is still mandatory — audit emission
    /// records governed decisions, never bypasses.
    /// </summary>
    public AuditEmission? AuditEmission { get; init; }

    public static CommandResult Success(IReadOnlyList<object> events, object? output = null, bool eventsRequirePersistence = true) =>
        new() { IsSuccess = true, EmittedEvents = events, Output = output, EventsRequirePersistence = eventsRequirePersistence };

    public static CommandResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
