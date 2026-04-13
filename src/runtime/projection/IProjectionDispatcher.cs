using Whycespace.Runtime.EventFabric;

namespace Whycespace.Runtime.Projection;

/// <summary>
/// Projection dispatcher contract. Dispatches event envelopes to registered
/// projection handlers after EventStore persistence and chain anchoring.
/// Projections do NOT depend on Kafka.
/// </summary>
public interface IProjectionDispatcher
{
    Task DispatchAsync(IReadOnlyList<EventEnvelope> envelopes);
}
