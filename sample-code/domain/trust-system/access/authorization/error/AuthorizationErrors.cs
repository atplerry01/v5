namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public static class AuthorizationErrors
{
    public static DomainException Unauthorized(string resource, string action)
        => new("AUTH.UNAUTHORIZED", $"Identity is not authorized to perform '{action}' on '{resource}'.");

    public static DomainException PolicyNotFound(string policyId)
        => new("AUTH.POLICY_NOT_FOUND", $"Authorization policy '{policyId}' not found.");

    public static DomainException Expired(Guid authorizationId)
        => new("AUTH.EXPIRED", $"Authorization '{authorizationId}' has expired.");
}
