namespace Whycespace.Shared.Contracts.Runtime;

public interface ISystemIntentDispatcher
{
    // phase1.5-S5.2.3 / TC-1 (DISPATCHER-CT-CONTRACT-01): same
    // CancellationToken propagation as ICommandDispatcher.
    // SystemIntentDispatcher forwards the token into
    // IRuntimeControlPlane.ExecuteAsync so the entire pipeline
    // becomes cancelable from the controller signature inward.
    Task<CommandResult> DispatchAsync(object command, DomainRoute route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Phase 2.5 — internal-only dispatch path for system-originated
    /// commands (workflow compensation, settlement completion, recovery
    /// work). Sets <c>CommandContext.IsSystem = true</c> so the
    /// <c>EnforcementGuard</c> bypasses the restriction gate — the
    /// enforcement middleware still evaluates locks and Critical+Block
    /// violations because those are hard-stop financial safety rails,
    /// but restrictions (which target user-initiated actions) do not
    /// block system work. ONLY workflow / recovery code should call
    /// this overload; controllers must use <see cref="DispatchAsync"/>.
    /// </summary>
    Task<CommandResult> DispatchSystemAsync(object command, DomainRoute route, CancellationToken cancellationToken = default);
}

/// <summary>
/// Domain routing metadata for canonical Kafka topic resolution.
/// Populated by the entry point (intent handler, controller) and threaded through
/// the dispatcher chain into CommandContext → EventEnvelope → TopicNameResolver.
/// </summary>
public sealed record DomainRoute(string Classification, string Context, string Domain);
