namespace Whycespace.Domain.BusinessSystem.Integration.Mapping;

public sealed class CanDisableSpecification
{
    public bool IsSatisfiedBy(MappingStatus status)
    {
        return status == MappingStatus.Active;
    }
}
