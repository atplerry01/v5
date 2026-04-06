namespace Whycespace.Domain.StructuralSystem.HumanCapital.Performance;

public sealed class PerformanceMetric
{
    public Guid Id { get; }
    public string MetricName { get; }
    public double Weight { get; }

    public PerformanceMetric(Guid id, string metricName, double weight)
    {
        Id = id;
        MetricName = metricName;
        Weight = weight;
    }
}
