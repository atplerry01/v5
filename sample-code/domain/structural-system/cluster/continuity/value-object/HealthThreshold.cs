namespace Whycespace.Domain.StructuralSystem.Cluster.Continuity;

public sealed record HealthThreshold
{
    public decimal Value { get; }

    public HealthThreshold(decimal value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(value), "HealthThreshold must be between 0 and 100.");

        Value = value;
    }
}
