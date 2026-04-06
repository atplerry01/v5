namespace Whycespace.Platform.Api.Intelligence.Geo.Distance;

public sealed record DistanceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record DistanceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
