namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public readonly record struct AccountId
{
    public Guid Value { get; }

    public AccountId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AccountId cannot be empty.", nameof(value));

        Value = value;
    }

    public static AccountId From(Guid value) => new(value);
}
