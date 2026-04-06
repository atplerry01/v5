namespace Whycespace.Projections.Intelligence.Simulation.Forecast;

public sealed record ForecastView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
