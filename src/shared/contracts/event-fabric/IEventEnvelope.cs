namespace Whyce.Shared.Contracts.EventFabric;

/// <summary>
/// Read-only contract for an event envelope. Defined in shared contracts so that
/// projection handlers in <c>src/projections/**</c> can consume envelopes without
/// taking a project reference on <c>src/runtime/**</c>. The concrete envelope record
/// lives in <c>src/runtime/event-fabric/EventEnvelope.cs</c> and implements this
/// interface.
///
/// EventVersion is exposed as a string here to keep the runtime <c>EventVersion</c>
/// type out of the shared contract surface. Consumers that need the structured
/// version should parse it themselves.
/// </summary>
public interface IEventEnvelope
{
    Guid EventId { get; }
    Guid AggregateId { get; }
    Guid CorrelationId { get; }
    string EventType { get; }
    string EventName { get; }
    string EventVersion { get; }
    string SchemaHash { get; }
    object Payload { get; }
    string ExecutionHash { get; }
    string PolicyHash { get; }
    DateTimeOffset Timestamp { get; }
    int SequenceNumber { get; }
    string Classification { get; }
    string Context { get; }
    string Domain { get; }
}
