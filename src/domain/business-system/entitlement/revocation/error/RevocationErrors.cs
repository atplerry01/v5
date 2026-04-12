namespace Whycespace.Domain.BusinessSystem.Entitlement.Revocation;

public static class RevocationErrors
{
    public static RevocationDomainException MissingId()
        => new("RevocationId is required and must not be empty.");

    public static RevocationDomainException MissingTargetId()
        => new("RevocationTargetId is required and must not be empty.");

    public static RevocationDomainException InvalidStateTransition(RevocationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static RevocationDomainException AlreadyRevoked(RevocationId id)
        => new($"Revocation '{id.Value}' has already been revoked.");

    public static RevocationDomainException AlreadyFinalized(RevocationId id)
        => new($"Revocation '{id.Value}' has already been finalized and cannot be modified.");
}

public sealed class RevocationDomainException : Exception
{
    public RevocationDomainException(string message) : base(message) { }
}
