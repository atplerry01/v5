namespace Whycespace.Domain.BusinessSystem.Integration.Mapping;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(MappingStatus status)
    {
        return status is MappingStatus.Defined or MappingStatus.Disabled;
    }
}
