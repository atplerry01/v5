namespace Whycespace.Platform.Api.Business.Logistic.Route;

public sealed record RouteRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RouteResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
