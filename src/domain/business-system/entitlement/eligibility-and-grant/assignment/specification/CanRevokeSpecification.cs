namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public sealed class CanRevokeSpecification
{
    public bool IsSatisfiedBy(AssignmentStatus status) => status is AssignmentStatus.Pending or AssignmentStatus.Active;
}
