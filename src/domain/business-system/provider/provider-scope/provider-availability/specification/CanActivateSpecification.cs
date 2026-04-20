namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ProviderAvailabilityStatus status) => status == ProviderAvailabilityStatus.Draft;
}
