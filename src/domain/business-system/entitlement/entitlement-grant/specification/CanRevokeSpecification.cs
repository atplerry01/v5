namespace Whycespace.Domain.BusinessSystem.Entitlement.EntitlementGrant;

public sealed class CanRevokeSpecification
{
    public bool IsSatisfiedBy(EntitlementGrantStatus status)
    {
        return status == EntitlementGrantStatus.Granted;
    }
}
