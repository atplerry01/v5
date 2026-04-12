namespace Whycespace.Domain.BusinessSystem.Portfolio.Performance;

public readonly record struct PerformanceId
{
    public Guid Value { get; }

    public PerformanceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PerformanceId value must not be empty.", nameof(value));

        Value = value;
    }
}
