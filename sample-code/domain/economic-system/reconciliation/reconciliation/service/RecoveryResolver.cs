namespace Whycespace.Domain.EconomicSystem.Reconciliation.Reconciliation;

public sealed class RecoveryResolver
{
    public IEnumerable<RecoveryInstruction> Resolve(ReconciliationResult result)
    {
        var actions = new List<RecoveryInstruction>();

        if (!result.IsBalanced)
            actions.Add(new RecoveryInstruction(result.TransactionId, "REBALANCE_LEDGER"));

        if (!result.IsSettled)
            actions.Add(new RecoveryInstruction(result.TransactionId, "RETRY_SETTLEMENT"));

        if (result.HasPathMismatch)
            actions.Add(new RecoveryInstruction(result.TransactionId, "REBUILD_PATH"));

        if (result.HasAmountMismatch)
            actions.Add(new RecoveryInstruction(result.TransactionId, "RECALCULATE_AMOUNT"));

        return actions;
    }
}
