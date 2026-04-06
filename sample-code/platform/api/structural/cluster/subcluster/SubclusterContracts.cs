namespace Whycespace.Platform.Api.Structural.Cluster.Subcluster;

public sealed record SubclusterRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SubclusterResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
