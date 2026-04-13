using Whycespace.Shared.Contracts.EventFabric;

namespace Whycespace.Shared.Contracts.Projection;

/// <summary>
/// Envelope-based projection handler contract. Lives in shared contracts so that
/// <c>src/projections/**</c> can implement it without referencing <c>src/runtime/**</c>.
///
/// This is distinct from the typed <c>Whycespace.Shared.Contracts.Infrastructure.Projection.IProjectionHandler&lt;T&gt;</c>
/// which is keyed on a specific event payload type. The envelope-based handler
/// receives the full envelope (including correlation, sequence, and metadata) and
/// is the contract used by the runtime ProjectionRegistry / ProjectionDispatcher.
///
/// Rules:
/// - Handlers MUST be idempotent (same event twice = same state)
/// - Handlers MUST NOT dispatch commands
/// - Handlers MUST NOT call aggregates or domain services
/// - Handlers MUST NOT depend on Kafka
/// - Handlers MUST support full rebuild from event stream
/// </summary>
public interface IEnvelopeProjectionHandler
{
    ProjectionExecutionPolicy ExecutionPolicy { get; }

    // phase1.5-S5.2.3 / TC-6 (PROJECTION-CT-CONTRACT-01): envelope-based
    // projection handler now consumes the worker's stoppingToken so a
    // hung handler can be unblocked at the database round-trip without
    // waiting for Kafka poll/session limits to intervene. Default keeps
    // source compatibility for handlers that have not yet been migrated.
    Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default);
}
