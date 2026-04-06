namespace Whycespace.Projections.Intelligence.Observability.Metric;

public sealed record MetricView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
