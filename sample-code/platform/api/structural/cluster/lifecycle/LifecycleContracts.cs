namespace Whycespace.Platform.Api.Structural.Cluster.Lifecycle;

public sealed record LifecycleRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record LifecycleResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
