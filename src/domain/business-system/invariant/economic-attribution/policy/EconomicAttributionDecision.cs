namespace Whycespace.Domain.BusinessSystem.Invariant.EconomicAttribution;

public enum EconomicAttributionOutcome
{
    Allowed,
    Denied
}

public enum EconomicAttributionReason
{
    None,
    MissingTransaction,
    MissingBusinessReference
}

public readonly record struct EconomicAttributionDecision
{
    public EconomicAttributionOutcome Outcome { get; }
    public EconomicAttributionReason Reason { get; }

    private EconomicAttributionDecision(EconomicAttributionOutcome outcome, EconomicAttributionReason reason)
    {
        Outcome = outcome;
        Reason = reason;
    }

    public static EconomicAttributionDecision Allow()
        => new(EconomicAttributionOutcome.Allowed, EconomicAttributionReason.None);

    public static EconomicAttributionDecision Deny(EconomicAttributionReason reason)
        => new(EconomicAttributionOutcome.Denied, reason);

    public bool IsAllowed => Outcome == EconomicAttributionOutcome.Allowed;
}
