namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ConstraintStatus status) => status == ConstraintStatus.Draft;
}
