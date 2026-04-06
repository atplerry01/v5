namespace Whycespace.Projections.Core.Reconciliation.ReconciliationException;

public interface IReconciliationExceptionViewRepository
{
    Task SaveAsync(ReconciliationExceptionReadModel model, CancellationToken ct = default);
    Task<ReconciliationExceptionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
