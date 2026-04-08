namespace Whyce.Runtime.Projection;

/// <summary>
/// Generic projection table writer. One instance per projection table —
/// domain bootstrap modules construct it with their schema/table/aggregate-type config
/// and inject it into the generic Kafka consumer worker.
///
/// Implementations MUST:
///   - perform read-modify-write with version increment
///   - resolve aggregate id from the deserialized event by convention (AggregateId property)
///   - serialize the event payload as JSONB state
///   - track correlation id from the message header
/// </summary>
public interface IPostgresProjectionWriter
{
    Task WriteAsync(
        string eventType,
        object @event,
        string correlationId,
        CancellationToken ct);
}
