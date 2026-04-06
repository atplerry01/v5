namespace Whycespace.Projections.Business.Integration.Synchronization;

public interface ISynchronizationViewRepository
{
    Task SaveAsync(SynchronizationReadModel model, CancellationToken ct = default);
    Task<SynchronizationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
