namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public readonly record struct AccountId
{
    public Guid Value { get; }

    public AccountId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AccountId value must not be empty.", nameof(value));

        Value = value;
    }
}
