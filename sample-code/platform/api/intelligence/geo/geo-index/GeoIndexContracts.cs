namespace Whycespace.Platform.Api.Intelligence.Geo.GeoIndex;

public sealed record GeoIndexRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record GeoIndexResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
