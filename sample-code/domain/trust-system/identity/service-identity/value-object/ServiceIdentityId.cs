using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

public sealed record ServiceIdentityId
{
    public Guid Value { get; }

    public ServiceIdentityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ServiceIdentityId cannot be empty.", nameof(value));
        Value = value;
    }

    public static ServiceIdentityId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public override string ToString() => Value.ToString();
}
