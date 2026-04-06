namespace Whycespace.Projections.Intelligence.Geo.RegionMapping;

public sealed record RegionMappingView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
