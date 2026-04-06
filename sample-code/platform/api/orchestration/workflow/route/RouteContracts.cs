namespace Whycespace.Platform.Api.Orchestration.Workflow.Route;

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
