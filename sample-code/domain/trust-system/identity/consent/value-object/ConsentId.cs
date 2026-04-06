using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.TrustSystem.Identity.Consent;

public sealed record ConsentId
{
    public Guid Value { get; }

    public ConsentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ConsentId cannot be empty.", nameof(value));
        Value = value;
    }

    public static ConsentId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public override string ToString() => Value.ToString();
}
