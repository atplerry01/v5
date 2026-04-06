namespace Whycespace.Shared.Contracts.Domain.Economic;

/// <summary>
/// Reconciliation context for graph-executed economic transactions (E17.7).
/// Identifies the transaction to reconcile. Runtime resolves expected vs actual
/// snapshots and passes them to the domain reconciliation engine.
/// </summary>
public interface IReconciliationContext
{
    Guid TransactionId { get; }
}
