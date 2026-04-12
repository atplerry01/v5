namespace Whycespace.Domain.BusinessSystem.Integration.Registry;

public sealed class CanDeactivateSpecification
{
    public bool IsSatisfiedBy(RegistryStatus status)
    {
        return status == RegistryStatus.Active;
    }
}
