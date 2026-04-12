namespace Whycespace.Domain.BusinessSystem.Integration.Registry;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(RegistryStatus status)
    {
        return status == RegistryStatus.Active;
    }
}
