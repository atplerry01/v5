namespace Whycespace.Shared.Contracts.Structural.Cluster.Authority;

public sealed record AuthorityReadModel
{
    public Guid AuthorityId { get; init; }
    public Guid ClusterReference { get; init; }
    public string AuthorityName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
    public Guid? AttachedClusterRef { get; init; }
    public DateTimeOffset? AttachedAt { get; init; }
    public bool BindingValidated { get; init; }
    public Guid? BindingParent { get; init; }
    public string? BindingParentState { get; init; }
}
