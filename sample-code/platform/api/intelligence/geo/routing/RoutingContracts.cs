namespace Whycespace.Platform.Api.Intelligence.Geo.Routing;

public sealed record RoutingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RoutingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
