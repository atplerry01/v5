namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public sealed class CanBindSpecification
{
    public bool IsSatisfiedBy(PolicyBindingStatus status) => status is PolicyBindingStatus.Draft or PolicyBindingStatus.Unbound;
}
