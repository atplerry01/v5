using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public sealed record AccessProfileId
{
    public Guid Value { get; }

    public AccessProfileId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AccessProfileId cannot be empty.", nameof(value));
        Value = value;
    }

    public static AccessProfileId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public override string ToString() => Value.ToString();
}
