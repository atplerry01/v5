namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(ServiceWindowStatus status) => status != ServiceWindowStatus.Archived;
}
