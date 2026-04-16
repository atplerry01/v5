using Whycespace.Shared.Contracts.EventFabric;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// Minimal IEventEnvelope used by integration-test append helpers when only
/// AggregateId + Payload are semantically meaningful (concurrency / ordering
/// tests against the in-memory and Postgres event stores). All other envelope
/// fields default to empty values.
/// </summary>
internal sealed class RawTestEnvelope : IEventEnvelope
{
    public Guid EventId { get; init; }
    public Guid AggregateId { get; init; }
    public Guid CorrelationId { get; init; }
    public Guid CausationId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string EventName { get; init; } = string.Empty;
    public string EventVersion { get; init; } = string.Empty;
    public string SchemaHash { get; init; } = string.Empty;
    public object Payload { get; init; } = new();
    public string ExecutionHash { get; init; } = string.Empty;
    public string PolicyHash { get; init; } = string.Empty;
    public string? PolicyVersion { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UnixEpoch;
    public int SequenceNumber { get; init; }
    public string Classification { get; init; } = string.Empty;
    public string Context { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
}

internal static class RawEnvelopes
{
    /// <summary>
    /// Wraps raw event payloads in <see cref="RawTestEnvelope"/> instances bound
    /// to the given aggregate id. Lets concurrency / ordering tests pass plain
    /// event objects without constructing full envelopes.
    /// </summary>
    public static IReadOnlyList<IEventEnvelope> Wrap(Guid aggregateId, params object[] events)
    {
        var envelopes = new IEventEnvelope[events.Length];
        for (var i = 0; i < events.Length; i++)
            envelopes[i] = new RawTestEnvelope { AggregateId = aggregateId, Payload = events[i] };
        return envelopes;
    }
}
