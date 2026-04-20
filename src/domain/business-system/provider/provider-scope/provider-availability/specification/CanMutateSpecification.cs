namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(ProviderAvailabilityStatus status) => status != ProviderAvailabilityStatus.Archived;
}
