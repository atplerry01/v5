namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(ConstraintStatus status) => status != ConstraintStatus.Archived;
}
