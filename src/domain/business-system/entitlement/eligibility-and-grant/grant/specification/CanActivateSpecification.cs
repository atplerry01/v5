namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(GrantStatus status) => status == GrantStatus.Pending;
}
