namespace Whycespace.Domain.EconomicSystem.Reconciliation.Reconciliation;

public sealed class ReconciliationResult
{
    public Guid TransactionId { get; }
    public bool IsBalanced { get; }
    public bool IsSettled { get; }
    public bool HasPathMismatch { get; }
    public bool HasAmountMismatch { get; }

    public ReconciliationResult(
        Guid transactionId,
        bool balanced,
        bool settled,
        bool pathMismatch,
        bool amountMismatch)
    {
        TransactionId = transactionId;
        IsBalanced = balanced;
        IsSettled = settled;
        HasPathMismatch = pathMismatch;
        HasAmountMismatch = amountMismatch;
    }

    public bool IsHealthy()
        => IsBalanced && IsSettled && !HasPathMismatch && !HasAmountMismatch;
}
