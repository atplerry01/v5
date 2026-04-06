namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public static class IdentityErrors
{
    public static DomainException AlreadyActive(Guid identityId)
        => new("IDENTITY_ALREADY_ACTIVE", $"IdentityAggregate '{identityId}' is already active.");

    public static DomainException NotFound(Guid identityId)
        => new("IDENTITY_NOT_FOUND", $"IdentityAggregate '{identityId}' was not found.");

    public static DomainException Deactivated(Guid identityId)
        => new("IDENTITY_DEACTIVATED", $"IdentityAggregate '{identityId}' is deactivated and cannot be modified.");
}
