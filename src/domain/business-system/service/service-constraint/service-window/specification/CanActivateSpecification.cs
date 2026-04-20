namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ServiceWindowStatus status) => status == ServiceWindowStatus.Draft;
}
