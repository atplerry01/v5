namespace Whycespace.Domain.IntelligenceSystem.Economic.Optimization;

public sealed record ImpactEstimate
{
    public decimal Value { get; }

    public ImpactEstimate(decimal value)
    {
        Value = value;
    }

    public static ImpactEstimate Zero => new(0);
    public bool IsPositive => Value > 0;
    public bool IsNegative => Value < 0;
}
