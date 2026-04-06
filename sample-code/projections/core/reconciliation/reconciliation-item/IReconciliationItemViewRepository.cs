namespace Whycespace.Projections.Core.Reconciliation.ReconciliationItem;

public interface IReconciliationItemViewRepository
{
    Task SaveAsync(ReconciliationItemReadModel model, CancellationToken ct = default);
    Task<ReconciliationItemReadModel?> GetAsync(string id, CancellationToken ct = default);
}
