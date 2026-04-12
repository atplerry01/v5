namespace Whycespace.Domain.BusinessSystem.Integration.Provider;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ProviderStatus status)
    {
        return status == ProviderStatus.Registered;
    }
}
