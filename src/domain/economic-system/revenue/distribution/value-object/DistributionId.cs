namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public readonly record struct DistributionId
{
    public Guid Value { get; }

    public DistributionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DistributionId cannot be empty.", nameof(value));
        Value = value;
    }

    public static DistributionId From(Guid value) => new(value);
}
