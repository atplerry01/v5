namespace Whycespace.Projections.Structural.Cluster.Lifecycle;

public interface ILifecycleViewRepository
{
    Task SaveAsync(LifecycleReadModel model, CancellationToken ct = default);
    Task<LifecycleReadModel?> GetAsync(string id, CancellationToken ct = default);
}
