namespace Whycespace.Platform.Api.Intelligence.Geo.Geofence;

public sealed record GeofenceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record GeofenceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
