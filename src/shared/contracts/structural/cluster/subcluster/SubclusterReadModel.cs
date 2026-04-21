namespace Whycespace.Shared.Contracts.Structural.Cluster.Subcluster;

public sealed record SubclusterReadModel
{
    public Guid SubclusterId { get; init; }
    public Guid ParentClusterReference { get; init; }
    public string SubclusterName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
    public Guid? AttachedClusterRef { get; init; }
    public DateTimeOffset? AttachedAt { get; init; }
    public bool BindingValidated { get; init; }
    public Guid? BindingParent { get; init; }
    public string? BindingParentState { get; init; }
}
