namespace Whycespace.Domain.EconomicSystem.Exchange.Rate;

public readonly record struct RateId
{
    public Guid Value { get; }

    public RateId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RateId cannot be empty.", nameof(value));

        Value = value;
    }

    public static RateId From(Guid value) => new(value);
}
