namespace Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;

public static class LedgerErrors
{
    public static InvalidOperationException AlreadySealed() =>
        new("Ledger is already sealed.");

    public static ArgumentException EmptyLedgerName() =>
        new("Ledger name must be non-empty.", nameof(LedgerDescriptor));
}
