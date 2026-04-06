using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.IntelligenceSystem.Observability.Trace;

/// <summary>
/// Uniquely identifies a distributed trace spanning multiple services and spans.
/// </summary>
public readonly record struct TraceId(Guid Value)
{
    public static TraceId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static readonly TraceId Empty = new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(TraceId id) => id.Value;
    public static implicit operator TraceId(Guid id) => new(id);
}
