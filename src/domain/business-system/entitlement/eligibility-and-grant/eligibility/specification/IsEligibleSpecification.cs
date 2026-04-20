namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public sealed class IsEligibleSpecification
{
    public bool IsSatisfiedBy(EligibilityStatus status) => status == EligibilityStatus.Eligible;
}
