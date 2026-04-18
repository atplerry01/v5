using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Ledger.Journal;

public static class JournalErrors
{
    public static DomainException UnbalancedJournal(Amount totalDebit, Amount totalCredit)
        => new($"Journal is unbalanced: total debit {totalDebit.Value} does not equal total credit {totalCredit.Value}.");

    public static DomainException InsufficientEntries(int count)
        => new($"Journal must have at least 2 entries, found {count}.");

    public static DomainException JournalAlreadyPosted()
        => new("Cannot modify a posted journal.");

    public static DomainException InvalidAmount()
        => new("Amount must be greater than zero.");

    public static DomainException InvalidDirection()
        => new("Direction must be Debit or Credit.");

    public static DomainException MissingEntryReference()
        => new("Entry must have a valid entry id.");

    public static DomainException MissingAccountReference()
        => new("Entry must reference a valid account.");

    public static DomainException CurrencyMismatch(Currency expected, Currency actual)
        => new($"Currency mismatch: expected {expected.Code} but received {actual.Code}.");

    public static DomainInvariantViolationException JournalBalanceInvariantViolation(Amount totalDebit, Amount totalCredit)
        => new($"Invariant violated: journal balance failed with debit {totalDebit.Value} and credit {totalCredit.Value}.");

    public static DomainInvariantViolationException NegativeEntryAmount()
        => new("Invariant violated: journal entry amount cannot be negative.");

    public static DomainException ControlPlaneOriginRequired()
        => new("Ledger journal posts must originate from an approved control plane " +
               "(transaction lifecycle workflow OR revenue payout pipeline). Direct API " +
               "or user-originated dispatches are rejected (Phase 4.5 T4.5.3).");

    // ── Phase 7 T7.4 — compensation invariants ───────────────────────
    public static DomainException CompensationReferenceRequired()
        => new("A Kind=Compensating journal requires a non-empty CompensationReference at creation.");

    public static DomainException CompensatingJournalCannotReferenceSelf()
        => new("CompensationReference.OriginalJournalId must differ from the compensating JournalId.");

    public static DomainInvariantViolationException CompensationMissingForCompensatingKind()
        => new("Invariant violated: Kind=Compensating requires a non-null CompensationReference.");

    public static DomainInvariantViolationException CompensationSetForStandardKind()
        => new("Invariant violated: Kind=Standard must not carry a CompensationReference.");

    public static DomainException OriginalJournalNotFound(Guid originalJournalId)
        => new($"Cannot post a compensating journal: original journal {originalJournalId} has no event history.");

    public static DomainException OriginalJournalNotPosted(Guid originalJournalId)
        => new($"Cannot post a compensating journal: original journal {originalJournalId} is not in Posted status.");

    public static DomainException OriginalJournalAlreadyCompensating(Guid originalJournalId)
        => new($"Cannot post a compensating journal against {originalJournalId}: the referenced journal is itself Kind=Compensating. Reflexive compensation is forbidden.");

    public static DomainException CompensationTotalsMismatch(
        Guid originalJournalId,
        decimal originalTotal,
        decimal compensatingTotal)
        => new($"Compensating journal totals {compensatingTotal} do not match original journal {originalJournalId} totals {originalTotal}. Reversal is not value-symmetric.");
}
