namespace Whycespace.Projections.Intelligence.Economic.Anomaly;

public sealed record AnomalyView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
