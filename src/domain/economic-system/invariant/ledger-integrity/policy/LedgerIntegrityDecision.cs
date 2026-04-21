namespace Whycespace.Domain.EconomicSystem.Invariant.LedgerIntegrity;

public enum LedgerIntegrityOutcome
{
    Allowed,
    Denied
}

public enum LedgerIntegrityReason
{
    None,
    MissingTransaction,
    NoLedgerEntries,
    NegativeAmount,
    Unbalanced
}

public readonly record struct LedgerIntegrityDecision
{
    public LedgerIntegrityOutcome Outcome { get; }
    public LedgerIntegrityReason Reason { get; }

    private LedgerIntegrityDecision(LedgerIntegrityOutcome outcome, LedgerIntegrityReason reason)
    {
        Outcome = outcome;
        Reason = reason;
    }

    public static LedgerIntegrityDecision Allow()
        => new(LedgerIntegrityOutcome.Allowed, LedgerIntegrityReason.None);

    public static LedgerIntegrityDecision Deny(LedgerIntegrityReason reason)
        => new(LedgerIntegrityOutcome.Denied, reason);

    public bool IsAllowed => Outcome == LedgerIntegrityOutcome.Allowed;
}
