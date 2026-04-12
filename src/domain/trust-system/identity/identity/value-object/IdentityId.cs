namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public readonly record struct IdentityId
{
    public Guid Value { get; }

    public IdentityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("IdentityId value must not be empty.", nameof(value));

        Value = value;
    }
}
