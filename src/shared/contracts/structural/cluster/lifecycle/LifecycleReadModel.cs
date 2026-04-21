namespace Whycespace.Shared.Contracts.Structural.Cluster.Lifecycle;

public sealed record LifecycleReadModel
{
    public Guid LifecycleId { get; init; }
    public Guid ClusterReference { get; init; }
    public string LifecycleName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
