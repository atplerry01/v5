namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public sealed class CanUnbindSpecification
{
    public bool IsSatisfiedBy(PolicyBindingStatus status) => status == PolicyBindingStatus.Bound;
}
