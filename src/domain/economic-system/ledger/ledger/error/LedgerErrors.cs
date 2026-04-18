using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public static class LedgerErrors
{
    public static DomainException DuplicateJournal(Guid journalId) =>
        new($"Journal '{journalId}' is already appended to this ledger.");

    public static DomainException InvalidJournalReference() =>
        new("Journal reference cannot be empty.");

    public static DomainException LedgerIsEmpty() =>
        new("Ledger has no posted journals to reconstruct from.");

    public static DomainException DirectBalanceMutationForbidden() =>
        new("Ledger balances cannot be set directly. They must be reconstructed from journal entries.");

    public static DomainException UnbalancedJournal(Guid journalId, decimal totalDebit, decimal totalCredit) =>
        new($"Journal '{journalId}' is unbalanced: debits {totalDebit:F2} != credits {totalCredit:F2}.");

    public static DomainException NegativeJournalTotal() =>
        new("Journal totals (debit, credit) cannot be negative.");

    public static DomainInvariantViolationException LedgerBalanceInvariantViolation(decimal totalDebit, decimal totalCredit) =>
        new($"Invariant violated: ledger-level balance broken. " +
            $"Cumulative debits {totalDebit:F2} != cumulative credits {totalCredit:F2} across all appended journals.");
}
