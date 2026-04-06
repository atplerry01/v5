namespace Whycespace.Platform.Api.Intelligence.Geo.RegionMapping;

public sealed record RegionMappingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RegionMappingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
