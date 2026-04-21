namespace Whycespace.Shared.Contracts.Structural.Cluster.Cluster;

public sealed record ClusterReadModel
{
    public Guid ClusterId { get; init; }
    public string ClusterName { get; init; } = string.Empty;
    public string ClusterType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
    public IReadOnlyCollection<Guid> ActiveAuthorityIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyCollection<Guid> ActiveAdministrationIds { get; init; } = Array.Empty<Guid>();
}
