namespace Whycespace.Shared.Contracts.Structural.Cluster.Administration;

public sealed record AdministrationReadModel
{
    public Guid AdministrationId { get; init; }
    public Guid ClusterReference { get; init; }
    public string AdministrationName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
    public Guid? AttachedClusterRef { get; init; }
    public DateTimeOffset? AttachedAt { get; init; }
    public bool BindingValidated { get; init; }
    public Guid? BindingParent { get; init; }
    public string? BindingParentState { get; init; }
}
