namespace Whycespace.Domain.BusinessSystem.Entitlement.Eligibility;

public sealed class CanEvaluateSpecification
{
    public bool IsSatisfiedBy(EligibilityStatus status)
    {
        return status == EligibilityStatus.Pending;
    }
}
