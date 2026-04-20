namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(ServiceDefinitionStatus status) => status != ServiceDefinitionStatus.Archived;
}
