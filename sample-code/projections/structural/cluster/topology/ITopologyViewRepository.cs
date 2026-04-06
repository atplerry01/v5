namespace Whycespace.Projections.Structural.Cluster.Topology;

public interface ITopologyViewRepository
{
    Task SaveAsync(TopologyReadModel model, CancellationToken ct = default);
    Task<TopologyReadModel?> GetAsync(string id, CancellationToken ct = default);
}
