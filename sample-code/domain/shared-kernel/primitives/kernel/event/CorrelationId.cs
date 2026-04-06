using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

/// <summary>
/// Tracks the originating request or workflow across event chains.
/// All events produced from the same user action share a CorrelationId.
/// </summary>
public readonly record struct CorrelationId(string Value)
{
    public static CorrelationId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed).ToString());
    public static readonly CorrelationId Empty = new(string.Empty);

    public bool IsEmpty => string.IsNullOrEmpty(Value);

    public override string ToString() => Value;

    public static implicit operator string(CorrelationId id) => id.Value;
    public static implicit operator CorrelationId(string id) => new(id);
}