namespace Whycespace.Domain.TrustSystem.Access.Grant;

public static class GrantErrors
{
    public static DomainException AlreadyGranted(Guid identityId, string resource, string permission)
        => new("GRANT.ALREADY_GRANTED", $"Identity '{identityId}' already has '{permission}' on '{resource}'.");

    public static DomainException NotFound(Guid grantId)
        => new("GRANT.NOT_FOUND", $"Access grant '{grantId}' not found.");
}
