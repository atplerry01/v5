namespace Whycespace.Projections.Intelligence.Capacity.AllocationForecast;

public sealed record AllocationForecastView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
