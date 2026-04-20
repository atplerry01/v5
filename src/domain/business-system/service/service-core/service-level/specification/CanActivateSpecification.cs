namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ServiceLevelStatus status) => status == ServiceLevelStatus.Draft;
}
