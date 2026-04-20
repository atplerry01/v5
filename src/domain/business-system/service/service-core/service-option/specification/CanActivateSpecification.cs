namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ServiceOptionStatus status) => status == ServiceOptionStatus.Draft;
}
