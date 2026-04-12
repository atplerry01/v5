namespace Whycespace.Domain.BusinessSystem.Entitlement.Revocation;

public sealed class CanRevokeSpecification
{
    public bool IsSatisfiedBy(RevocationStatus status)
    {
        return status == RevocationStatus.Active;
    }
}
