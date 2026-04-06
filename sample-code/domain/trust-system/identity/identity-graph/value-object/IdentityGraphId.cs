using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

public sealed record IdentityGraphId
{
    public Guid Value { get; }

    public IdentityGraphId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("IdentityGraphId cannot be empty.", nameof(value));
        Value = value;
    }

    public static IdentityGraphId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public override string ToString() => Value.ToString();
}
