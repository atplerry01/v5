namespace Whycespace.Shared.Contracts.Structural.Cluster.Provider;

public sealed record ProviderReadModel
{
    public Guid ProviderId { get; init; }
    public Guid ClusterReference { get; init; }
    public string ProviderName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
    public Guid? AttachedClusterRef { get; init; }
    public DateTimeOffset? AttachedAt { get; init; }
}
