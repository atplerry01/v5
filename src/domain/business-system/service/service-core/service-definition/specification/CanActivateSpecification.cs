namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ServiceDefinitionStatus status) => status == ServiceDefinitionStatus.Draft;
}
