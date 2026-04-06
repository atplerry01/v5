namespace Whycespace.Projections.Structural.Cluster.Subcluster;

public interface ISubclusterViewRepository
{
    Task SaveAsync(SubclusterReadModel model, CancellationToken ct = default);
    Task<SubclusterReadModel?> GetAsync(string id, CancellationToken ct = default);
}
