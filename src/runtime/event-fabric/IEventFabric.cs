using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.EventFabric;

/// <summary>
/// Event Fabric contract. The single entry point for all post-execution
/// event processing in the runtime. ControlPlane → Fabric is SINGLE and NON-BYPASSABLE.
///
/// The fabric is an ORCHESTRATOR ONLY. It delegates to:
/// - EventStoreService (persistence)
/// - ChainAnchorService (immutable ledger)
/// - ProjectionDispatcher (read model updates)
/// - OutboxService (external distribution)
/// </summary>
public interface IEventFabric
{
    Task ProcessAsync(IReadOnlyList<object> domainEvents, CommandContext context);
}
