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
    Task ProcessAsync(IReadOnlyList<object> domainEvents, CommandContext context);

    /// <summary>
    /// Processes an audit emission with explicit aggregate-id and routing
    /// overrides. Used for cross-cutting events (e.g. policy decisions) that
    /// must be persisted to a stream distinct from the command's aggregate.
    /// </summary>
    Task ProcessAuditAsync(AuditEmission audit, CommandContext context);
}
