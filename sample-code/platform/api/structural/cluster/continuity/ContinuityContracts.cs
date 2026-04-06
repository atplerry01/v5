namespace Whycespace.Platform.Api.Structural.Cluster.Continuity;

public sealed record ContinuityRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ContinuityResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
