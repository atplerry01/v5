namespace Whycespace.Domain.ContentSystem.Invariant.BroadcastStreamBinding;

public enum BroadcastStreamBindingOutcome
{
    Allowed,
    Denied
}

public enum BroadcastStreamBindingReason
{
    None,
    MissingStream,
    StreamNotFound,
    StreamTerminal
}

public readonly record struct BroadcastStreamBindingDecision
{
    public BroadcastStreamBindingOutcome Outcome { get; }
    public BroadcastStreamBindingReason Reason { get; }

    private BroadcastStreamBindingDecision(BroadcastStreamBindingOutcome outcome, BroadcastStreamBindingReason reason)
    {
        Outcome = outcome;
        Reason = reason;
    }

    public static BroadcastStreamBindingDecision Allow()
        => new(BroadcastStreamBindingOutcome.Allowed, BroadcastStreamBindingReason.None);

    public static BroadcastStreamBindingDecision Deny(BroadcastStreamBindingReason reason)
        => new(BroadcastStreamBindingOutcome.Denied, reason);

    public bool IsAllowed => Outcome == BroadcastStreamBindingOutcome.Allowed;
    public bool IsDenied => Outcome == BroadcastStreamBindingOutcome.Denied;
}
