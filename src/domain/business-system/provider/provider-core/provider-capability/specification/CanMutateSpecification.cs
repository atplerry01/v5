namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public sealed class CanMutateSpecification
{
    public bool IsSatisfiedBy(CapabilityStatus status) => status != CapabilityStatus.Archived;
}
