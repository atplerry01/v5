namespace Whycespace.Platform.Api.Structural.Cluster.Topology;

public sealed record TopologyRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TopologyResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
