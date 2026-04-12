namespace Whycespace.Domain.BusinessSystem.Integration.Provider;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(ProviderStatus status)
    {
        return status == ProviderStatus.Active;
    }
}
