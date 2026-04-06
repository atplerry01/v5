using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public sealed record VerificationId
{
    public Guid Value { get; }

    public VerificationId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("VerificationId cannot be empty.", nameof(value));
        Value = value;
    }

    public static VerificationId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public override string ToString() => Value.ToString();
}
