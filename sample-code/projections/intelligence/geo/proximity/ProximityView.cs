namespace Whycespace.Projections.Intelligence.Geo.Proximity;

public sealed record ProximityView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
