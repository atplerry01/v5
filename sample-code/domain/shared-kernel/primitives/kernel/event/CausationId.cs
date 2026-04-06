using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

/// <summary>
/// Identifies the direct cause (parent event or command) that triggered this event.
/// Enables causal chain reconstruction for debugging and replay.
/// </summary>
public readonly record struct CausationId(string Value)
{
    public static CausationId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed).ToString());
    public static readonly CausationId Empty = new(string.Empty);

    public bool IsEmpty => string.IsNullOrEmpty(Value);

    public override string ToString() => Value;

    public static implicit operator string(CausationId id) => id.Value;
    public static implicit operator CausationId(string id) => new(id);
}