namespace Whycespace.Domain.ContentSystem.Invariant.Ownership;

public enum ContentOwnershipOutcome
{
    Allowed,
    Denied
}

public enum ContentOwnershipReason
{
    None,
    MissingContent,
    MissingOwner
}

public readonly record struct ContentOwnershipDecision
{
    public ContentOwnershipOutcome Outcome { get; }
    public ContentOwnershipReason Reason { get; }

    private ContentOwnershipDecision(ContentOwnershipOutcome outcome, ContentOwnershipReason reason)
    {
        Outcome = outcome;
        Reason = reason;
    }

    public static ContentOwnershipDecision Allow()
        => new(ContentOwnershipOutcome.Allowed, ContentOwnershipReason.None);

    public static ContentOwnershipDecision Deny(ContentOwnershipReason reason)
        => new(ContentOwnershipOutcome.Denied, reason);

    public bool IsAllowed => Outcome == ContentOwnershipOutcome.Allowed;
}
