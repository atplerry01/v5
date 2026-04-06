namespace Whycespace.Projections.Intelligence.Observability.Health;

public sealed record HealthView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
