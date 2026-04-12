namespace Whycespace.Domain.BusinessSystem.Entitlement.EntitlementGrant;

public sealed class CanGrantSpecification
{
    public bool IsSatisfiedBy(EntitlementGrantStatus status)
    {
        return status == EntitlementGrantStatus.Pending;
    }
}
