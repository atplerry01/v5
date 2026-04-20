namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(ServiceOptionStatus status) => status != ServiceOptionStatus.Archived;
}
