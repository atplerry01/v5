namespace Whycespace.Domain.EconomicSystem.Reconciliation.Reconciliation;

public sealed class ActualExecutionSnapshot
{
    public Guid TransactionId { get; }
    public IReadOnlyCollection<Guid> LedgerAccountsInvolved { get; }
    public decimal TotalDebits { get; }
    public decimal TotalCredits { get; }
    public bool SettlementCompleted { get; }

    public ActualExecutionSnapshot(
        Guid transactionId,
        IEnumerable<Guid> accounts,
        decimal totalDebits,
        decimal totalCredits,
        bool settlementCompleted)
    {
        TransactionId = transactionId;
        LedgerAccountsInvolved = accounts.Distinct().ToList().AsReadOnly();
        TotalDebits = totalDebits;
        TotalCredits = totalCredits;
        SettlementCompleted = settlementCompleted;
    }
}
