namespace Whycespace.Domain.BusinessSystem.Entitlement.Revocation;

public sealed class IsRevokedSpecification
{
    public bool IsSatisfiedBy(RevocationStatus status)
    {
        return status == RevocationStatus.Revoked;
    }
}
