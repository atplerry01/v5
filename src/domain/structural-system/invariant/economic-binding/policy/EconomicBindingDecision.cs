namespace Whycespace.Domain.StructuralSystem.Invariant.EconomicBinding;

public enum EconomicBindingOutcome
{
    Allowed,
    Denied
}

public enum EconomicBindingReason
{
    None,
    MissingEconomicEntity,
    MissingStructuralOwner
}

public readonly record struct EconomicBindingDecision
{
    public EconomicBindingOutcome Outcome { get; }
    public EconomicBindingReason Reason { get; }

    private EconomicBindingDecision(EconomicBindingOutcome outcome, EconomicBindingReason reason)
    {
        Outcome = outcome;
        Reason = reason;
    }

    public static EconomicBindingDecision Allow()
        => new(EconomicBindingOutcome.Allowed, EconomicBindingReason.None);

    public static EconomicBindingDecision Deny(EconomicBindingReason reason)
        => new(EconomicBindingOutcome.Denied, reason);

    public bool IsAllowed => Outcome == EconomicBindingOutcome.Allowed;
}
