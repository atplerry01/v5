namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public readonly record struct PayoutId
{
    public Guid Value { get; }

    public PayoutId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PayoutId cannot be empty.", nameof(value));
        Value = value;
    }

    public static PayoutId From(Guid value) => new(value);
}
