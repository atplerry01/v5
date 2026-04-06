using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

/// <summary>
/// Strongly-typed identifier for domain events.
/// Deterministic — derived from SHA256(AggregateId + EventType + Version + SequenceKey).
/// Same inputs always produce the same EventId, enabling replay verification.
/// </summary>
public readonly record struct EventId(Guid Value)
{
    public static readonly EventId Empty = new(Guid.Empty);

    /// <summary>
    /// Generates a deterministic EventId from aggregate context.
    /// SHA256(aggregateId:eventType:version:sequenceKey) → first 16 bytes → GUID.
    /// </summary>
    public static EventId Deterministic(Guid aggregateId, string eventType, int version, string sequenceKey)
    {
        var input = $"{aggregateId}:{eventType}:{version}:{sequenceKey}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        // Take first 16 bytes of SHA256 to form a deterministic GUID
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        // Set version nibble to 5 (SHA-based UUID) and variant bits
        guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x50);
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);
        return new EventId(new Guid(guidBytes));
    }

    /// <summary>
    /// Generates a deterministic EventId from string components.
    /// Used when AggregateId is not yet assigned (pre-creation events).
    /// </summary>
    public static EventId Deterministic(string aggregateId, string eventType, int version, DateTimeOffset timestamp)
        => Deterministic(
            Guid.TryParse(aggregateId, out var guid) ? guid : DeriveGuid(aggregateId),
            eventType,
            version,
            timestamp.ToUnixTimeMilliseconds().ToString());

    /// <summary>
    /// Derives a stable GUID from a string key using SHA256.
    /// </summary>
    private static Guid DeriveGuid(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        return new Guid(guidBytes);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(EventId id) => id.Value;
    public static implicit operator EventId(Guid id) => new(id);
}