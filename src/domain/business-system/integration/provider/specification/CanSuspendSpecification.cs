namespace Whycespace.Domain.BusinessSystem.Integration.Provider;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(ProviderStatus status)
    {
        return status == ProviderStatus.Active;
    }
}
