namespace Whycespace.Domain.BusinessSystem.Entitlement.Eligibility;

public sealed class IsEligibleSpecification
{
    public bool IsSatisfiedBy(EligibilityStatus status)
    {
        return status == EligibilityStatus.Eligible;
    }
}
