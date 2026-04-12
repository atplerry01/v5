using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Entry;

public static class EntryErrors
{
    public static DomainException InvalidAmount()
        => new("Amount must be greater than zero.");

    public static DomainException MissingJournalReference()
        => new("Entry must belong to a journal.");

    public static DomainException MissingAccountReference()
        => new("Entry must reference an account.");

    public static DomainException InvalidDirection()
        => new("Direction must be Debit or Credit.");

    public static DomainException EntryIsImmutable()
        => new("Ledger entry cannot be modified after recording.");

    public static DomainInvariantViolationException NegativeAmount()
        => new("Invariant violated: entry amount cannot be negative.");
}
