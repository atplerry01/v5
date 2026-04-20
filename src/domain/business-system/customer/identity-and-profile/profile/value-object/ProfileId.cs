namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public readonly record struct ProfileId
{
    public Guid Value { get; }

    public ProfileId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProfileId value must not be empty.", nameof(value));

        Value = value;
    }
}
