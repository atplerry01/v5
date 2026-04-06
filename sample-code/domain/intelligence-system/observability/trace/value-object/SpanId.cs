using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.IntelligenceSystem.Observability.Trace;

/// <summary>
/// Uniquely identifies a single span within a distributed trace.
/// </summary>
public readonly record struct SpanId(Guid Value)
{
    public static SpanId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static readonly SpanId Empty = new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SpanId id) => id.Value;
    public static implicit operator SpanId(Guid id) => new(id);
}
