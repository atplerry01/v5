namespace Whycespace.Domain.BusinessSystem.Integration.Mapping;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(MappingStatus status)
    {
        return status == MappingStatus.Active;
    }
}
