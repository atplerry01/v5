namespace Whycespace.Projections.CoreSystem.ReconciliationStatus;

public interface IReconciliationStatusViewRepository
{
    Task SaveAsync(ReconciliationStatusReadModel model, CancellationToken ct = default);
    Task<ReconciliationStatusReadModel?> GetAsync(string id, CancellationToken ct = default);
}
