using System.Security.Cryptography;
using System.Text;
using Whycespace.Shared.Contracts.EventFabric;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Immutable event envelope. Every domain event passing through the Event Fabric
/// is wrapped in this envelope with deterministic metadata.
///
/// Implements <see cref="IEventEnvelope"/> so projection handlers in
/// <c>src/projections/**</c> can consume envelopes through the shared contract
/// without referencing the runtime project.
///
/// All events MUST include:
/// - EventId (deterministic, SHA256-based)
/// - EventName (type name)
/// - EventVersion (schema version)
/// - SchemaHash (hash of the event schema)
/// - ExecutionHash (fingerprint of the execution)
/// - PolicyHash (hash of the policy decision)
/// </summary>
public sealed record EventEnvelope : IEventEnvelope
{
    public required Guid EventId { get; init; }
    public required Guid AggregateId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required Guid CausationId { get; init; }
    public required string EventType { get; init; }
    public required string EventName { get; init; }
    public required EventVersion EventVersion { get; init; }
    public required string SchemaHash { get; init; }
    public required object Payload { get; init; }
    public required string ExecutionHash { get; init; }
    public required string PolicyHash { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public int SequenceNumber { get; init; }

    // Domain routing metadata — used by TopicNameResolver for canonical Kafka topic resolution
    public string Classification { get; init; } = string.Empty;
    public string Context { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;

    // Explicit interface implementation: shared contract exposes EventVersion as a
    // string so the runtime EventVersion type is not lifted into shared contracts.
    string IEventEnvelope.EventVersion => EventVersion.ToString();

    public static Guid GenerateDeterministicId(Guid correlationId, string eventType, int sequenceNumber)
    {
        var input = $"{correlationId}:{eventType}:{sequenceNumber}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(hash.AsSpan(0, 16));
    }
}
