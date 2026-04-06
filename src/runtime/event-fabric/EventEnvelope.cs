using System.Security.Cryptography;
using System.Text;

namespace Whyce.Runtime.EventFabric;

/// <summary>
/// Immutable event envelope. Every domain event passing through the Event Fabric
/// is wrapped in this envelope with deterministic metadata.
///
/// All events MUST include:
/// - EventId (deterministic, SHA256-based)
/// - EventName (type name)
/// - EventVersion (schema version)
/// - SchemaHash (hash of the event schema)
/// - ExecutionHash (fingerprint of the execution)
/// - PolicyHash (hash of the policy decision)
/// </summary>
public sealed record EventEnvelope
{
    public required Guid EventId { get; init; }
    public required Guid AggregateId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required string EventType { get; init; }
    public required string EventName { get; init; }
    public required EventVersion EventVersion { get; init; }
    public required string SchemaHash { get; init; }
    public required object Payload { get; init; }
    public required string ExecutionHash { get; init; }
    public required string PolicyHash { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public int SequenceNumber { get; init; }

    public static Guid GenerateDeterministicId(Guid correlationId, string eventType, int sequenceNumber)
    {
        var input = $"{correlationId}:{eventType}:{sequenceNumber}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(hash.AsSpan(0, 16));
    }
}
