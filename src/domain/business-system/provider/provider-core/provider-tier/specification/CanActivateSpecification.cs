namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ProviderTierStatus status) => status == ProviderTierStatus.Draft;
}
