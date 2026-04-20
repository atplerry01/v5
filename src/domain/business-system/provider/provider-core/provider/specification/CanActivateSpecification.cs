namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.Provider;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ProviderStatus status) => status == ProviderStatus.Draft;
}
