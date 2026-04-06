namespace Whycespace.Platform.Api.Structural.Cluster.Cluster;

public sealed record ClusterRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ClusterResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
