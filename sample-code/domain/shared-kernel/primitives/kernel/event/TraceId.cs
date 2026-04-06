using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

/// <summary>
/// Identifies the distributed trace spanning multiple bounded contexts.
/// Shared across all events, workflows, and sagas originating from the same
/// cross-domain interaction, enabling end-to-end observability.
/// </summary>
public readonly record struct TraceId(string Value)
{
    public static TraceId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed).ToString());
    public static readonly TraceId Empty = new(string.Empty);

    public bool IsEmpty => string.IsNullOrEmpty(Value);

    public override string ToString() => Value;

    public static implicit operator string(TraceId id) => id.Value;
    public static implicit operator TraceId(string id) => new(id);
}
