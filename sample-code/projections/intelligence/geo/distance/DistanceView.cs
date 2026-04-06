namespace Whycespace.Projections.Intelligence.Geo.Distance;

public sealed record DistanceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
