namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

/// <summary>
/// Domain service for inspecting the references carried by a committed
/// or initiated transaction. Returns typed iteration over references by
/// kind for downstream consumers; the service itself is stateless and
/// has zero cross-domain dependencies.
/// </summary>
public sealed class TransactionOrchestrationService
{
    public IEnumerable<TransactionReference> ReferencesOfKind(
        TransactionAggregate transaction,
        string kind)
    {
        if (transaction is null) yield break;
        if (string.IsNullOrWhiteSpace(kind)) yield break;

        foreach (var r in transaction.References)
        {
            if (string.Equals(r.Kind, kind, StringComparison.Ordinal))
                yield return r;
        }
    }

    public bool HasReferenceOfKind(TransactionAggregate transaction, string kind)
    {
        foreach (var _ in ReferencesOfKind(transaction, kind)) return true;
        return false;
    }
}
