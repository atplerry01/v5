namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public static class GrantErrors
{
    public static GrantDomainException MissingId()
        => new("GrantId is required and must not be empty.");

    public static GrantDomainException MissingSubject()
        => new("Grant requires a subject ref.");

    public static GrantDomainException MissingTarget()
        => new("Grant requires a target ref.");

    public static GrantDomainException InvalidStateTransition(GrantStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static GrantDomainException AlreadyTerminal(GrantId id, GrantStatus status)
        => new($"Grant '{id.Value}' is already terminal ({status}) and cannot be reactivated.");

    public static GrantDomainException ExpiryInPast()
        => new("GrantExpiry cannot already be in the past at the moment of creation or activation.");
}

public sealed class GrantDomainException : Exception
{
    public GrantDomainException(string message) : base(message) { }
}
