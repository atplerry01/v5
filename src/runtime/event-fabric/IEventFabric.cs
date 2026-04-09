using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.EventFabric;

/// <summary>
/// Event Fabric contract. The single entry point for all post-execution
/// event processing in the runtime. ControlPlane → Fabric is SINGLE and NON-BYPASSABLE.
///
/// The fabric is an ORCHESTRATOR ONLY. It delegates to:
/// - EventStoreService (persistence)
/// - ChainAnchorService (immutable ledger)
/// - OutboxService (Kafka relay — projections consume from Kafka ONLY)
/// </summary>
public interface IEventFabric
{
    // phase1.5-S5.2.3 / TC-2 (CHAIN-ANCHOR-WAIT-TIMEOUT-01): both fabric
    // entry points now accept the request/host-shutdown CancellationToken
    // so it can reach ChainAnchorService.AnchorAsync and bound the global
    // commit serializer wait. The token is wait-only in this pass —
    // chain-store I/O cancellation threading is deferred to TC-3.
    Task ProcessAsync(
        IReadOnlyList<object> domainEvents,
        CommandContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes an audit emission with explicit aggregate-id and routing
    /// overrides. Used for cross-cutting events (e.g. policy decisions) that
    /// must be persisted to a stream distinct from the command's aggregate.
    /// </summary>
    Task ProcessAuditAsync(
        AuditEmission audit,
        CommandContext context,
        CancellationToken cancellationToken = default);
}
