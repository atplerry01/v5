using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed record TrustProfileId
{
    public Guid Value { get; }

    public TrustProfileId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TrustProfileId cannot be empty.", nameof(value));
        Value = value;
    }

    public static TrustProfileId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public override string ToString() => Value.ToString();
}
