namespace Whycespace.Projections.Business.Execution.Lifecycle;

public interface ILifecycleViewRepository
{
    Task SaveAsync(LifecycleReadModel model, CancellationToken ct = default);
    Task<LifecycleReadModel?> GetAsync(string id, CancellationToken ct = default);
}
