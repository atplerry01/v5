namespace Whycespace.Shared.Contracts.Control.Observability.SystemMetric;

public sealed record SystemMetricReadModel
{
    public Guid MetricId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public IReadOnlyList<string> LabelNames { get; init; } = [];
    public bool IsDeprecated { get; init; }
}
