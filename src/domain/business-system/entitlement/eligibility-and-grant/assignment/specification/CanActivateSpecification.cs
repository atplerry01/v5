namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(AssignmentStatus status) => status == AssignmentStatus.Pending;
}
