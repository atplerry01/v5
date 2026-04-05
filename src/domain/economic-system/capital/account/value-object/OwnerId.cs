namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public readonly record struct OwnerId
{
    public Guid Value { get; }

    public OwnerId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OwnerId cannot be empty.", nameof(value));

        Value = value;
    }

    public static OwnerId From(Guid value) => new(value);
}
