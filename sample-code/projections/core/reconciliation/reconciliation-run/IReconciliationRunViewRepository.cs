namespace Whycespace.Projections.Core.Reconciliation.ReconciliationRun;

public interface IReconciliationRunViewRepository
{
    Task SaveAsync(ReconciliationRunReadModel model, CancellationToken ct = default);
    Task<ReconciliationRunReadModel?> GetAsync(string id, CancellationToken ct = default);
}
