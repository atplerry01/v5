namespace Whycespace.Domain.ContentSystem.Invariant.SessionStreamAccess;

public enum SessionStreamAccessOutcome
{
    Allowed,
    Denied
}

public enum SessionStreamAccessReason
{
    None,
    MissingStream,
    MissingSubject,
    AccessGrantMissing,
    AccessGrantRevoked,
    AccessGrantExpired,
    AccessGrantRestricted
}

public readonly record struct SessionStreamAccessDecision
{
    public SessionStreamAccessOutcome Outcome { get; }
    public SessionStreamAccessReason Reason { get; }

    private SessionStreamAccessDecision(SessionStreamAccessOutcome outcome, SessionStreamAccessReason reason)
    {
        Outcome = outcome;
        Reason = reason;
    }

    public static SessionStreamAccessDecision Allow()
        => new(SessionStreamAccessOutcome.Allowed, SessionStreamAccessReason.None);

    public static SessionStreamAccessDecision Deny(SessionStreamAccessReason reason)
        => new(SessionStreamAccessOutcome.Denied, reason);

    public bool IsAllowed => Outcome == SessionStreamAccessOutcome.Allowed;
    public bool IsDenied => Outcome == SessionStreamAccessOutcome.Denied;
}
