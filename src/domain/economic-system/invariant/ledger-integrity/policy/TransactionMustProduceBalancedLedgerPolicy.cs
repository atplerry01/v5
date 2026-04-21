namespace Whycespace.Domain.EconomicSystem.Invariant.LedgerIntegrity;

public sealed class TransactionMustProduceBalancedLedgerPolicy
{
    public LedgerIntegrityDecision Decide(
        Guid transactionId,
        int entryCount,
        decimal totalDebit,
        decimal totalCredit)
    {
        if (transactionId == Guid.Empty)
            return LedgerIntegrityDecision.Deny(LedgerIntegrityReason.MissingTransaction);

        if (entryCount <= 0)
            return LedgerIntegrityDecision.Deny(LedgerIntegrityReason.NoLedgerEntries);

        if (totalDebit < 0m || totalCredit < 0m)
            return LedgerIntegrityDecision.Deny(LedgerIntegrityReason.NegativeAmount);

        if (totalDebit != totalCredit)
            return LedgerIntegrityDecision.Deny(LedgerIntegrityReason.Unbalanced);

        return LedgerIntegrityDecision.Allow();
    }
}
