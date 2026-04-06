namespace Whycespace.Platform.Api.Intelligence.Geo.Proximity;

public sealed record ProximityRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ProximityResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
