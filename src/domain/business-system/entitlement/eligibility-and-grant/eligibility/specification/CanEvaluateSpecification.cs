namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public sealed class CanEvaluateSpecification
{
    public bool IsSatisfiedBy(EligibilityStatus status) => status == EligibilityStatus.Pending;
}
