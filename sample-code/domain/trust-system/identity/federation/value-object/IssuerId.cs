using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Strongly-typed identifier for a federation issuer.
/// </summary>
public sealed record IssuerId
{
    public Guid Value { get; }

    public IssuerId(Guid value)
    {
        Guard.AgainstDefault(value);
        Value = value;
    }

    public static IssuerId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static implicit operator Guid(IssuerId id) => id.Value;
    public override string ToString() => Value.ToString();
}
