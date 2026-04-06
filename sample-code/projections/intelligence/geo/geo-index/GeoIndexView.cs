namespace Whycespace.Projections.Intelligence.Geo.GeoIndex;

public sealed record GeoIndexView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
