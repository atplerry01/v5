namespace Whycespace.Projections.Structural.Cluster.Cluster;

public interface IClusterViewRepository
{
    Task SaveAsync(ClusterReadModel model, CancellationToken ct = default);
    Task<ClusterReadModel?> GetAsync(string id, CancellationToken ct = default);
}
