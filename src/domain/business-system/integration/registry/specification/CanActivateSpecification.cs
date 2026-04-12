namespace Whycespace.Domain.BusinessSystem.Integration.Registry;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(RegistryStatus status)
    {
        return status == RegistryStatus.Defined || status == RegistryStatus.Deactivated;
    }
}
