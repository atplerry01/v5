namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public sealed class CanExpireSpecification
{
    public bool IsSatisfiedBy(GrantStatus status) => status is GrantStatus.Pending or GrantStatus.Active;
}
