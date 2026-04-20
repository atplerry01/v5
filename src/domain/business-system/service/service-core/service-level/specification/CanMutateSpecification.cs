namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(ServiceLevelStatus status) => status != ServiceLevelStatus.Archived;
}
