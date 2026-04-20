namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(ProviderTierStatus status) => status != ProviderTierStatus.Archived;
}
