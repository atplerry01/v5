using Whyce.Runtime.EventFabric;

namespace Whyce.Runtime.Projection;

/// <summary>
/// Projection handler contract. Handles a single event envelope to update a read model.
///
/// Rules:
/// - Handlers MUST be idempotent (same event twice = same state)
/// - Handlers MUST NOT dispatch commands
/// - Handlers MUST NOT call aggregates or domain services
/// - Handlers MUST NOT depend on Kafka
/// - Handlers MUST support full rebuild from event stream
/// </summary>
public interface IProjectionHandler
{
    ProjectionExecutionPolicy ExecutionPolicy { get; }
    Task HandleAsync(EventEnvelope envelope);
}
