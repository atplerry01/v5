namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.Provider;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(ProviderStatus status) => status != ProviderStatus.Archived;
}
