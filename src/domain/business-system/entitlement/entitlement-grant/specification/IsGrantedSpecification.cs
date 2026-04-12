namespace Whycespace.Domain.BusinessSystem.Entitlement.EntitlementGrant;

public sealed class IsGrantedSpecification
{
    public bool IsSatisfiedBy(EntitlementGrantStatus status)
    {
        return status == EntitlementGrantStatus.Granted;
    }
}
