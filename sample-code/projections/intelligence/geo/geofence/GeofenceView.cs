namespace Whycespace.Projections.Intelligence.Geo.Geofence;

public sealed record GeofenceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
