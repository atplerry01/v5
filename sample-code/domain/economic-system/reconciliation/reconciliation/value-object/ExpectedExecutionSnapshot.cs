namespace Whycespace.Domain.EconomicSystem.Reconciliation.Reconciliation;

public sealed class ExpectedExecutionSnapshot
{
    public Guid TransactionId { get; }
    public IReadOnlyCollection<Guid> Path { get; }
    public decimal Amount { get; }
    public string Currency { get; }

    public ExpectedExecutionSnapshot(
        Guid transactionId,
        IEnumerable<Guid> path,
        decimal amount,
        string currency)
    {
        TransactionId = transactionId;
        Path = path.Distinct().ToList().AsReadOnly();
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }
}
