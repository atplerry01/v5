namespace Whycespace.Domain.BusinessSystem.Entitlement.Revocation;

public sealed class CanFinalizeSpecification
{
    public bool IsSatisfiedBy(RevocationStatus status)
    {
        return status == RevocationStatus.Revoked;
    }
}
