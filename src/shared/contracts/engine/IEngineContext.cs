namespace Whycespace.Shared.Contracts.Engine;

public interface IEngineContext
{
    object Command { get; }
    Guid AggregateId { get; }

    /// <summary>
    /// Enforcement constraint stamped by ExecutionGuardMiddleware for the
    /// current command. Format: "Restricted:{scope}" when the subject has
    /// an active restriction, severity level ("High", "Medium", …) when an
    /// escalation is active, or null when unconstrained. Engine handlers
    /// that participate in the restriction surface call
    /// <c>EnforcementGuard.RequireNotRestricted</c> at the top of execution
    /// to hard-reject restricted subjects (defense-in-depth; the middleware
    /// already rejects before reaching the engine).
    /// </summary>
    string? EnforcementConstraint { get; }

    /// <summary>
    /// Phase 2.5 — system-origin bypass flag. Propagated from
    /// <c>CommandContext.IsSystem</c>. When true, the command was
    /// dispatched by internal system code (workflow compensation,
    /// settlement completion, recovery), and <c>EnforcementGuard</c>
    /// bypasses restriction enforcement so the command can complete
    /// against a subject that has become restricted mid-flight.
    /// User-originated commands dispatched via the standard API path
    /// always observe <c>IsSystem = false</c>.
    /// </summary>
    bool IsSystem { get; }

    Task<object> LoadAggregateAsync(Type aggregateType);
    void EmitEvents(IReadOnlyList<object> events);
    IReadOnlyList<object> EmittedEvents { get; }
}
