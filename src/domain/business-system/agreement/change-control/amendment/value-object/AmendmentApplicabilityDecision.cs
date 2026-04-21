namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;

public enum AmendmentApplicabilityOutcome
{
    Allowed,
    Denied
}

public enum AmendmentApplicabilityReason
{
    None,
    AmendmentNotInDraft,
    TargetContractNotActive
}

public readonly record struct AmendmentApplicabilityDecision
{
    public AmendmentApplicabilityOutcome Outcome { get; }
    public AmendmentApplicabilityReason Reason { get; }

    private AmendmentApplicabilityDecision(AmendmentApplicabilityOutcome outcome, AmendmentApplicabilityReason reason)
    {
        Outcome = outcome;
        Reason = reason;
    }

    public static AmendmentApplicabilityDecision Allow()
        => new(AmendmentApplicabilityOutcome.Allowed, AmendmentApplicabilityReason.None);

    public static AmendmentApplicabilityDecision Deny(AmendmentApplicabilityReason reason)
        => new(AmendmentApplicabilityOutcome.Denied, reason);

    public bool IsAllowed => Outcome == AmendmentApplicabilityOutcome.Allowed;
}
