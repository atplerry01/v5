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
}
