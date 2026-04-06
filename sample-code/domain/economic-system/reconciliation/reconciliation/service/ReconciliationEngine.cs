namespace Whycespace.Domain.EconomicSystem.Reconciliation.Reconciliation;

public sealed class ReconciliationEngine
{
    public ReconciliationResult Evaluate(
        ExpectedExecutionSnapshot expected,
        ActualExecutionSnapshot actual)
    {
        var balanced = actual.TotalDebits == actual.TotalCredits;

        var settled = actual.SettlementCompleted;

        var pathMismatch = !expected.Path.All(actual.LedgerAccountsInvolved.Contains);

        var amountMismatch = expected.Amount != actual.TotalDebits;

        return new ReconciliationResult(
            expected.TransactionId,
            balanced,
            settled,
            pathMismatch,
            amountMismatch);
    }
}
