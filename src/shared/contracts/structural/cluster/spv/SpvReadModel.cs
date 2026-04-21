namespace Whycespace.Shared.Contracts.Structural.Cluster.Spv;

public sealed record SpvReadModel
{
    public Guid SpvId { get; init; }
    public Guid ClusterReference { get; init; }
    public string SpvName { get; init; } = string.Empty;
    public string SpvType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
    public Guid? AttachedClusterRef { get; init; }
    public DateTimeOffset? AttachedAt { get; init; }
}
