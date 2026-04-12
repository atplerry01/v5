namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public readonly record struct LimitId
{
    public Guid Value { get; }

    public LimitId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LimitId cannot be empty.", nameof(value));
        Value = value;
    }

    public static LimitId From(Guid value) => new(value);
}
