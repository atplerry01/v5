namespace Whycespace.Domain.BusinessSystem.Entitlement.EntitlementGrant;

public static class EntitlementGrantErrors
{
    public static EntitlementGrantDomainException MissingId()
        => new("EntitlementGrantId is required and must not be empty.");

    public static EntitlementGrantDomainException MissingAssignment()
        => new("EntitlementAssignment is required and must not be null.");

    public static EntitlementGrantDomainException InvalidStateTransition(EntitlementGrantStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static EntitlementGrantDomainException AlreadyGranted(EntitlementGrantId id)
        => new($"EntitlementGrant '{id.Value}' has already been granted.");

    public static EntitlementGrantDomainException AlreadyRevoked(EntitlementGrantId id)
        => new($"EntitlementGrant '{id.Value}' has already been revoked.");

    public static EntitlementGrantDomainException CannotRevokeBeforeGrant()
        => new("Cannot revoke an entitlement grant that has not been granted.");
}

public sealed class EntitlementGrantDomainException : Exception
{
    public EntitlementGrantDomainException(string message) : base(message) { }
}
